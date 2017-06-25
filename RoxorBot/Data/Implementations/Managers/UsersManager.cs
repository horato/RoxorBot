using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Extensions;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib;
using TwitchLib.Models.Client;
using User = RoxorBot.Data.Model.Database.Entities.User;

namespace RoxorBot.Data.Implementations.Managers
{
    public class UsersManager : IUsersManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly ITwitchLibTranslationService _translationService;
        private readonly IUsersRepository _usersRepository;
        private readonly IUserWrapperFactory _userWrapperFactory;
        private readonly Dictionary<Guid, UserWrapper> _users;
        private readonly List<string> _superAdmins;

        public int UsersCount => _users.Count;

        public UsersManager(IEventAggregator aggregator, ITwitchLibTranslationService translationService, IUsersRepository usersRepository, IUserWrapperFactory factory)
        {
            _aggregator = aggregator;
            _translationService = translationService;
            _usersRepository = usersRepository;
            _userWrapperFactory = factory;
            _aggregator.GetEvent<AddLogEvent>().Publish("Loading UsersManager...");

            _users = LoadUsers();
            _superAdmins = new List<string> { "roxork0", "horato2" };
            _aggregator.GetEvent<ChatUserJoinedEvent>().Subscribe(OnChatUserJoined);
            _aggregator.GetEvent<ChatUserLeftEvent>().Subscribe(OnChatUserLeft);
            _aggregator.GetEvent<ModeratorsListReceivedEvent>().Subscribe(OnModeratorsListReceived);
        }

        private Dictionary<Guid, UserWrapper> LoadUsers()
        {
            var ret = new Dictionary<Guid, UserWrapper>();
            var users = _usersRepository.GetAll();
            foreach (var user in users)
                ret.Add(user.Id, _userWrapperFactory.CreateNew(user));

            return ret;
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
            ChangeOnlineStatus(user.ValueName, true);
            if (obj.IsModerator)
                SetAsModerator(user.ValueName);

            _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
        }

        private void OnChatUserLeft(ChatUserLeftEventEventArgs obj)
        {
            var user = GetUser(obj.Name);
            if (user == null)
                return;

            ChangeOnlineStatus(user.ValueName, false);
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
                ChangeOnlineStatus(u.ValueName, true);
            }
        }

        public UserWrapper AddOrGetUser(string user, Role role)
        {
            lock (_users)
            {
                var u = GetUser(user);
                if (u != null)
                    return u;

                u = _userWrapperFactory.CreateNew(user, user.ToLower(), role, false, 0, false, null, false);
                _users.Add(u.Id, u);
                return u;
            }
        }

        public void AddNewUsers(IEnumerable<string> users, Role role)
        {
            var names = users?.ToList();
            if (names == null)
                return;

            lock (_users)
            {
                var newUsers = _userWrapperFactory.CreateNew(names, role, false, 0, false, null, false);
                foreach (var newUser in newUsers)
                    _users.Add(newUser.Id, newUser);
            }
        }

        public void AllowUser(string nick)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.IsAllowed = true;
            UpdateModel(user.Model);
        }

        public void RevokeAllowUser(string nick)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.IsAllowed = false;
            UpdateModel(user.Model);
        }

        public void ChangeOnlineStatus(string nick, bool isOnline)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.IsOnline = isOnline;
            UpdateModel(user.Model);
        }

        public List<UserWrapper> GetAllUsers()
        {
            lock (_users)
                return _users.Values.ToList();
        }

        public UserWrapper GetUser(string nick)
        {
            lock (_users)
                return _users.Values.SingleOrDefault(x => x.ValueName == nick.ToLower());
        }

        public UserWrapper GetUser(Guid id)
        {
            if (_users.ContainsKey(id))
                return _users[id];

            return null;
        }

        public bool IsAdmin(string name)
        {
            if (IsSuperAdmin(name))
                return true;

            var user = GetUser(name);
            return IsAdmin(user);
        }

        public bool IsAdmin(UserWrapper user)
        {
            if (user == null)
                return false;

            if (IsSuperAdmin(user.ValueName))
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
            var user = GetUser(name);
            return IsAllowed(user);
        }

        public bool IsAllowed(UserWrapper user)
        {
            if (user == null)
                return false;

            return user.IsAllowed;
        }

        public void Save(UserWrapper user)
        {
            if (user == null)
                return;

            UpdateModel(user.Model);
        }

        public void SaveAll()
        {
            lock (_users)
                _usersRepository.SaveAll(_users.Select(x => x.Value.Model));
        }

        private void SetAsModerator(string userName)
        {
            var u = GetUser(userName);
            if (u == null || u.Role != Role.Viewers)
                return;

            u.Role = Role.Moderators;
            UpdateModel(u.Model);
        }

        private void UpdateModel(User model)
        {
            if (model == null)
                return;

            _usersRepository.Save(model);
            _usersRepository.FlushSession();
        }

        public UserWrapper AddNewUser(string name, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            lock (_users)
            {
                var user = GetUser(name);
                if (user != null)
                    return null;

                var newUser = _userWrapperFactory.CreateNew(name, name.ToLower(), role, isOnline, points, isFollower, isFollowerSince, isAllowed);
                _users.Add(newUser.Id, newUser);
                return newUser;
            }
        }

        public void UpdateUser(Guid id, string name, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            var user = GetUser(id);
            if (user == null)
                return;

            user.VisibleName = name;
            user.ValueName = name.ToLower();
            user.Role = role;
            user.IsOnline = isOnline;
            user.Points = points;
            user.IsFollower = isFollower;
            user.IsFollowerSince = isFollowerSince;
            user.IsAllowed = isAllowed;
            UpdateModel(user.Model);
        }

        public void RemoveUser(Guid id)
        {
            var user = GetUser(id);
            if (user == null)
                return;

            RemoveUser(user);
        }

        public void RemoveUser(UserWrapper user)
        {
            if (user == null)
                return;

            _usersRepository.Remove(user.Model);
            _usersRepository.FlushSession();
            lock (_users)
                _users.Remove(user.Id);
        }
    }
}
