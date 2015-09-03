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
            users = new List<User>();
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// </summary>
        /// <param name="list"></param>
        /// <param name="role"></param>
        public void initUsers(string[] list, Role role)
        {
            Logger.Log("Loading UsersManager...");
            foreach (string s in list)
                addUser(s, role);
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        public void addUser(string user, Role role)
        {
            if (!users.Any(x => x.InternalName == user.ToLower()))
                users.Add(new User { Name = user, InternalName = user.ToLower(), Role = role });
        }

        /// <summary>
        /// Don't forget to call OnListChanged!
        /// </summary>
        /// <param name="user"></param>
        public void removeUser(String user)
        {
            users.RemoveAll(x => x.InternalName == user.ToLower());
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

        public void clear()
        {
            users.Clear();
            users = null;
            _instance = null;
        }

        public static UsersManager getInstance()
        {
            if (_instance == null)
                _instance = new UsersManager();
            return _instance;
        }
    }
}
