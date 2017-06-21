using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Factories;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Factories.Wrapper
{
    public class UserCommandWrapperFactory : IUserCommandWrapperFactory
    {
        private readonly IUserCommandsProvider _provider;

        public UserCommandWrapperFactory(IUserCommandsProvider provider)
        {
            _provider = provider;
        }

        public UserCommandWrapper CreateNew(UserCommand model)
        {
            return new UserCommandWrapper(model);
        }

        public UserCommandWrapper CreateNew(string command, string reply)
        {
            var model = _provider.CreateNew(command, reply);
            return CreateNew(model);
        }
    }
}
