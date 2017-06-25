using System;
using System.Collections.Generic;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IUsersManager
    {
        int UsersCount { get; }
        void InitUsers(JoinedChannel channel);
        UserWrapper AddOrGetUser(string user, Role role);
        UserWrapper AddNewUser(string name, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
        void AddNewUsers(IEnumerable<string> users, Role role);
        void UpdateUser(Guid id, string name, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
        void AllowUser(string nick);
        void RevokeAllowUser(string nick);
        void ChangeOnlineStatus(string user, bool isOnline);
        List<UserWrapper> GetAllUsers();
        UserWrapper GetUser(string nick);
        UserWrapper GetUser(Guid id);
        void RemoveUser(Guid id);
        void RemoveUser(UserWrapper user);
        bool IsAdmin(string name);
        bool IsAdmin(UserWrapper user);
        bool IsSuperAdmin(string user);
        bool IsAllowed(string name);
        bool IsAllowed(UserWrapper user);
        void Save(UserWrapper user);
        void SaveAll();
    }
}
