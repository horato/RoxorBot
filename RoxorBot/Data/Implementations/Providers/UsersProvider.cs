using System;
using System.Collections.Generic;
using System.Linq;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Providers
{
    public class UsersProvider : IUsersProvider
    {
        private readonly IUsersRepository _repository;
        private readonly IUserFactory _factory;

        public UsersProvider(IUsersRepository usersRepository, IUserFactory userFactory)
        {
            _repository = usersRepository;
            _factory = userFactory;
        }

        public User GetUser(Guid id)
        {
            return _repository.FindById(id);
        }

        public User CreateNew(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            var user = _factory.CreateNew(visibleName, valueName, role, isOnline, points, isFollower, isFollowerSince, isAllowed);
            _repository.Save(user);
            _repository.FlushSession();
            return user;
        }

        public IEnumerable<User> CreateNew(IEnumerable<string> names, Role role, bool isOnline, int points, bool isFollower, DateTime? isfollowerSince, bool isAllowed)
        {
            var users = names?.Select(x => _factory.CreateNew(x, x.ToLower(), role, isOnline, points, isFollower, isfollowerSince, isAllowed)).ToList();
            _repository.Create(users);
            return users;
        }
    }
}
