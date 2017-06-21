using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories
{
    public interface IUserWrapperFactory
    {
        UserWrapper CreateNew(User model);
        UserWrapper CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed);
    }
}
