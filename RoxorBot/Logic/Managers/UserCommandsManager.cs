using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Logic.Managers
{
    public class UserCommandsManager : IUserCommandsManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUserCommandWrapperFactory _wrapperFactory;
        private readonly IUserCommandsRepository _commandsRepository;
        private readonly Dictionary<Guid, UserCommandWrapper> _commands;
        public int CommandsCount => _commands.Count;

        public UserCommandsManager(IEventAggregator aggregator, IUserCommandWrapperFactory wrapperFactory, IUserCommandsRepository commandsRepository)
        {
            _aggregator = aggregator;
            _wrapperFactory = wrapperFactory;
            _commandsRepository = commandsRepository;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing UserCommandsManager...");
            _commands = LoadCommands();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + CommandsCount + " user commands from database.");
        }

        public void AddCommand(string command, string reply)
        {
            var cmd = _wrapperFactory.CreateNew(command.ToLower(), reply);
            _commands.Add(cmd.Id, cmd);
        }

        public void UpdateCommand(Guid id, string command, string reply)
        {
            var cmd = GetCommand(id);
            if (cmd == null)
                return;

            cmd.Command = command;
            cmd.Reply = reply;
            UpdateModel(cmd.Model);
        }

        private void UpdateModel(UserCommand model)
        {
            if (model == null)
                return;

            _commandsRepository.Save(model);
            _commandsRepository.FlushSession();
        }

        public bool RemoveCommand(string command)
        {
            var c = _commands.Values.LastOrDefault(x => string.Equals(x.Command, command, StringComparison.CurrentCultureIgnoreCase));
            if (c == null)
                return false;

            RemoveCommand(c);
            return true;
        }

        public void RemoveCommand(UserCommandWrapper command)
        {
            if (command == null)
                return;

            _commandsRepository.Remove(command.Model);
            _commandsRepository.FlushSession();
            _commands.Remove(command.Id);
        }

        private bool CommandsContains(string command)
        {
            return _commands.Values.Any(x => string.Equals(x.Command, command, StringComparison.CurrentCultureIgnoreCase));
        }

        private Dictionary<Guid, UserCommandWrapper> LoadCommands()
        {
            var commands = _commandsRepository.GetAll();
            var temp = new Dictionary<Guid, UserCommandWrapper>();

            foreach (var command in commands)
                temp.Add(command.Id, _wrapperFactory.CreateNew(command));

            return temp;
        }

        public List<UserCommandWrapper> GetAllCommands()
        {
            return _commands.Values.ToList();
        }

        public bool IsUserCommand(string command)
        {
            return CommandsContains(command);
        }

        private UserCommandWrapper GetCommand(Guid id)
        {
            if (!_commands.ContainsKey(id))
                return null;

            return _commands[id];
        }
    }
}
