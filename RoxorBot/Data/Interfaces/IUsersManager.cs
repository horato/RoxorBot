using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Interfaces
{
    public interface IUsersManager
    {
        int UsersCount { get; }
        void InitUsers(JoinedChannel channel);
        User AddOrGetUser(string user, Role role);
        void AllowUser(string nick);
        void RevokeAllowUser(string nick);
        void ChangeOnlineStatus(string user, bool isOnline);
        List<User> GetAllUsers();
        User GetUser(string nick);
        bool IsAdmin(string name);
        bool IsAdmin(User user);
        bool IsSuperAdmin(string user);
        bool IsAllowed(string name);
        bool IsAllowed(User user);
    }
}
