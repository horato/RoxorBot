using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Factories;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Factories
{
    public class UserWrapperFactory : IUserWrapperFactory
    {
        private readonly IUsersProvider _provider;

        public UserWrapperFactory(IUsersProvider provider)
        {
            _provider = provider;
        }

        public UserWrapper CreateNew(User model)
        {
            return new UserWrapper(model);
        }

        public UserWrapper CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            var model = _provider.CreateNew(visibleName, valueName, role, isOnline, points, isFollower, isFollowerSince, isAllowed);
            return CreateNew(model);
        }
    }
}
