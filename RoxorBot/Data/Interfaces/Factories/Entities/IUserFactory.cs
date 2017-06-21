using System;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories.Entities
{
    public interface IUserFactory
    {
        User CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
    }
}
