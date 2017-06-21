using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces
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
