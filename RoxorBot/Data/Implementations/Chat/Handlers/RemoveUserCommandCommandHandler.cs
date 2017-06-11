using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class RemoveUserCommandCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IUserCommandsManager _userCommandsManager;
        private readonly IUsersManager _usersManager;

        public RemoveUserCommandCommandHandler(IChatManager chatManager, IUserCommandsManager commandsManager, IUsersManager usersManager)
        {
            _chatManager = chatManager;
            _userCommandsManager = commandsManager;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.DelComm;
        }

        public void Handle(ChatCommand command)
        {
            //if (command?.ChatMessage == null)
            //    return;
            //if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
            //    return;
            //if (!_usersManager.IsAdmin(command.ChatMessage.Username))
            //    return;

            //var c = command.ArgumentsAsList.First();
            //if (_userCommandsManager.RemoveCommand(c))
            //    _chatManager.SendChatMessage(command.ChatMessage.Username + ": Command " + c + " removed.");
        }
    }
}
