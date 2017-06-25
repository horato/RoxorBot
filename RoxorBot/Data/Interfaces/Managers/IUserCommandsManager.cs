using System;
using System.Collections.Generic;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IUserCommandsManager
    {
        void AddCommand(string command, string reply);
        void UpdateCommand(Guid id, string command, string reply);
        bool RemoveCommand(string command);
        void RemoveCommand(UserCommandWrapper id);
        int CommandsCount { get; }
        List<UserCommandWrapper> GetAllCommands();
        bool IsUserCommand(string command);
    }
}
