using System;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Factories.Entities
{
    public class UserFactory : IUserFactory
    {
        public User CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            return new User(visibleName, valueName, role, isOnline, points, isFollower, isFollowerSince, isAllowed);
        }
    }
}
