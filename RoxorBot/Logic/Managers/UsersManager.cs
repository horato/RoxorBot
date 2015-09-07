using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class UsersManager
    {
        private static UsersManager _instance;
        private List<User> users;

        private UsersManager()
        {
            Logger.Log("Loading UsersManager...");

            users = new List<User>();
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// Added user is automatically set as online with 0 points!
        /// </summary>
        /// <param name="list"></param>
        /// <param name="role"></param>
        public void initUsers(string[] list, Role role)
        {
            foreach (string s in list)
            {
                var u = addUser(s, role);
                u.isOnline = true;
            }
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        public User addUser(string user, Role role)
        {
            var u = getUser(user);

            if (u == null)
            {
                u = new User { Name = user, InternalName = user.ToLower(), Role = role, isOnline = false, Points = 0, IsFollower = false };
                users.Add(u);
            }

            return u;
        }

        /// <summary>
        /// Don't forget to call OnListChanged!<br>
        /// This resets reward timer!
        /// </summary>
        /// <param name="user"></param>
        public void changeOnlineStatus(string user, bool isOnline)
        {
            var u = users.Find(x => x.InternalName == user.ToLower());
            if (u == null)
                return;

            u.isOnline = isOnline;
            //u.RewardTimer = 0;
        }

        public List<User> getAllUsers()
        {
            return users;
        }

        public User getUser(string nick)
        {
            return users.Find(x => x.InternalName == nick.ToLower());
        }

        public bool isAdmin(string name)
        {
            var user = users.Find(x => x.InternalName == name.ToLower());
            return isAdmin(user);
        }

        public bool isAdmin(User user)
        {
            if (user == null)
                return false;

            return user.Role != Role.Viewers;
        }

        public int getUsersCount()
        {
            return users.Count;
        }

        public static UsersManager getInstance()
        {
            if (_instance == null)
                _instance = new UsersManager();
            return _instance;
        }
    }
}
