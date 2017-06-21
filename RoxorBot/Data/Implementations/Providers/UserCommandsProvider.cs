using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Providers
{
    public class UserCommandsProvider : IUserCommandsProvider
    {
        private readonly IUserCommandsRepository _repository;
        private readonly IUserCommandFactory _factory;

        public UserCommandsProvider(IUserCommandsRepository repository, IUserCommandFactory factory)
        {
            _repository = repository;
            _factory = factory;
        }

        public UserCommand GetAutomatedMessage(Guid id)
        {
            return _repository.FindById(id);
        }

        public UserCommand CreateNew(string command, string reply)
        {
            var model = _factory.CreateNew(command, reply);
            _repository.Save(model);
            _repository.FlushSession();
            return model;
        }
    }
}
