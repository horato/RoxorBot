using System;
using System.Collections.Generic;
using System.Linq;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Implementations.Factories.Wrapper
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

        public IEnumerable<UserWrapper> CreateNew(IEnumerable<string> names, Role role, bool isOnline, int points, bool isFollower, DateTime? isfollowerSince, bool isAllowed)
        {
            var models = _provider.CreateNew(names, role, isOnline, points, isFollower, isfollowerSince, isAllowed);
            return models.Select(CreateNew).ToList();
        }
    }
}
