using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class UsersManager : IUsersManager
    {
        private readonly ILogger _logger;
        private readonly IDatabaseManager _databaseManager;
        private readonly List<User> _users;
        private readonly List<string> _superAdmins;

        public int UsersCount => _users.Count;

        public UsersManager(ILogger logger, IDatabaseManager databaseManager)
        {
            _logger = logger;
            _databaseManager = databaseManager;
            _logger.Log("Loading UsersManager...");

            _users = new List<User>();
            _superAdmins = new List<string>() { "roxork0", "horato2" };
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// Added user is automatically set as online with 0 points!
        /// </summary>
        /// <param name="list"></param>
        /// <param name="role"></param>
        public void InitUsers(string[] list, Role role)
        {
            foreach (var s in list)
            {
                var u = AddUser(s, role);
                u.isOnline = true;
            }
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        public User AddUser(string user, Role role)
        {
            var u = GetUser(user);

            if (u == null)
            {
                u = new User { Name = user, InternalName = user.ToLower(), Role = role, isOnline = false, Points = 0, IsFollower = false };
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

            user.isAllowed = true;
            _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO allowedUsers (name, allowed) VALUES (\"" + user.InternalName + "\", 1);");
        }

        public void RevokeAllowUser(string nick)
        {
            var user = GetUser(nick);
            if (user == null)
                return;

            user.isAllowed = false;
            _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO allowedUsers (name, allowed) VALUES (\"" + user.InternalName + "\", 0);");
        }

        /// <summary>
        /// Don't forget to call OnListChanged!<br>
        /// This resets reward timer!
        /// </summary>
        /// <param name="user"></param>
        public void ChangeOnlineStatus(string user, bool isOnline)
        {
            var u = _users.Find(x => x.InternalName == user.ToLower());
            if (u == null)
                return;

            u.isOnline = isOnline;
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
            return _superAdmins.Contains(user.ToLower());
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

            return user.isAllowed;
        }

        public int getUsersCount()
        {
            return _users.Count;
        }
    }
}
