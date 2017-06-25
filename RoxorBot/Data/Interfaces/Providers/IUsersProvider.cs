using System;
using System.Collections.Generic;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IUsersProvider
    {
        User GetUser(Guid id);
        User CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
        IEnumerable<User> CreateNew(IEnumerable<string> names, Role role, bool isOnline, int points, bool isFollower, DateTime? isfollowerSince, bool isAllowed);
    }
}
