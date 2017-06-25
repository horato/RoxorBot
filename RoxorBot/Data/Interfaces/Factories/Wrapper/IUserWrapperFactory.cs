using System;
using System.Collections.Generic;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Factories.Wrapper
{
    public interface IUserWrapperFactory
    {
        UserWrapper CreateNew(User model);
        UserWrapper CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
        IEnumerable<UserWrapper> CreateNew(IEnumerable<string> names, Role role, bool isOnline, int points, bool isFollower, DateTime? isfollowerSince, bool isAllowed);
    }
}
