using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Extensions;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Model;
using TwitchLib;
using TwitchLib.Models.Client;

namespace RoxorBot.Logic.Managers
{
    public class UsersManager : IUsersManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly ILogger _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly ITwitchLibTranslationService _translationService;
        private readonly List<User> _users;
        private readonly List<string> _superAdmins;

        public int UsersCount => _users.Count;

        public UsersManager(IEventAggregator aggregator, ILogger logger, IDatabaseManager databaseManager, ITwitchLibTranslationService translationService)
        {
            _aggregator = aggregator;
            _logger = logger;
            _databaseManager = databaseManager;
            _translationService = translationService;
            _logger.Log("Loading UsersManager...");

            _users = new List<User>();
            _superAdmins = new List<string> { "roxork0", "horato2" };
            _aggregator.GetEvent<ChatUserJoinedEvent>().Subscribe(OnChatUserJoined);
            _aggregator.GetEvent<ChatUserLeftEvent>().Subscribe(OnChatUserLeft);
            _aggregator.GetEvent<ModeratorsListReceivedEvent>().Subscribe(OnModeratorsListReceived);
        }

        private void OnModeratorsListReceived(List<string> e)
        {
            foreach (var user in e)
                SetAsModerator(user);

            _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
        }

        private void OnChatUserJoined(ChatUserJoinedEventArgs obj)
        {
            var user = AddOrGetUser(obj.Name, Role.Viewers);
            ChangeOnlineStatus(user.InternalName, true);
            if (obj.IsModerator)
                SetAsModerator(user.InternalName);

            _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
        }

        private void OnChatUserLeft(ChatUserLeftEventEventArgs obj)
        {
            var user = GetUser(obj.Name);
            if (user == null)
                return;

            ChangeOnlineStatus(user.InternalName, false);
            _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
        }

        /// <summary>
        /// Added user is automatically set as online with 0 points!
        /// </summary>
        /// <param name="channel"></param>
        public void InitUsers(JoinedChannel channel)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));

            var users = TwitchAPI.Undocumented.GetChatters(channel.Channel).WaitAndReturn();
            foreach (var user in users)
            {
                var type = _translationService.TranslateUserType(user.UserType);
                var u = AddOrGetUser(user.Username, type);
                u.IsOnline = true;
            }
        }

        public User AddOrGetUser(string user, Role role)
        {
            var u = GetUser(user);
            if (u == null)
            {
                u = new User { Name = user, InternalName = user.ToLower(), Role = role, IsOnline = false, Points = 0, IsFollower = false };
                lock (_users)
                    _users.Add(u);
            }

            return u;
        }

        public void AllowUser(string nick)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.IsAllowed = true;
            _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO allowedUsers (name, allowed) VALUES (\"" + user.InternalName + "\", 1);");
        }

        public void RevokeAllowUser(string nick)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.IsAllowed = false;
            _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO allowedUsers (name, allowed) VALUES (\"" + user.InternalName + "\", 0);");
        }

        public void ChangeOnlineStatus(string user, bool isOnline)
        {
            var u = GetUser(user);
            if (u == null)
                return;

            u.IsOnline = isOnline;
            //u.RewardTimer = 0;
        }

        public List<User> GetAllUsers()
        {
            return _users;
        }

        public User GetUser(string nick)
        {
            return _users.Find(x => x.InternalName == nick.ToLower());
        }

        public bool IsAdmin(string name)
        {
            if (IsSuperAdmin(name))
                return true;

            var user = _users.Find(x => x.InternalName == name.ToLower());
            return IsAdmin(user);
        }

        public bool IsAdmin(User user)
        {
            if (user == null)
                return false;

            if (IsSuperAdmin(user.InternalName))
                return true;

            return user.Role != Role.Viewers;
        }

        /// <summary>
        /// Todo someday
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool IsSuperAdmin(string user)
        {
            return _superAdmins.Any(x => string.Equals(x, user, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool IsAllowed(string name)
        {
            var user = _users.Find(x => x.InternalName == name.ToLower());
            return IsAllowed(user);
        }

        public bool IsAllowed(User user)
        {
            if (user == null)
                return false;

            return user.IsAllowed;
        }

        private void SetAsModerator(string userName)
        {
            var u = GetUser(userName);
            if (u == null || u.Role != Role.Viewers)
                return;

            u.Role = Role.Moderators;
        }
    }
}
