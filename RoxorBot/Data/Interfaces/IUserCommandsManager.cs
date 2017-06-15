﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;

namespace RoxorBot.Data.Interfaces
{
    public interface IUserCommandsManager
    {
        void AddCommand(string command, string reply, int id = 0);
        bool RemoveCommand(string command);
        void RemoveCommand(int id);
        int CommandsCount { get; }
        List<UserCommand> GetAllCommands();
        bool IsUserCommand(string command);
    }
}
