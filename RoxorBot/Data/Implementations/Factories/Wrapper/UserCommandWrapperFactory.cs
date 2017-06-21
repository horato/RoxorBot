using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

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
