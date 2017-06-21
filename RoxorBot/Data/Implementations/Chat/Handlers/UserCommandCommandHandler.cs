using System;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class UserCommandCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IUserCommandsManager _commandsManager;

        public UserCommandCommandHandler(IChatManager chatManager, IUserCommandsManager commandsManager)
        {
            _chatManager = chatManager;
            _commandsManager = commandsManager;
        }

        public bool CanHandle(string command)
        {
            return _commandsManager.IsUserCommand(command);
        }

        public void Handle(ChatCommand command)
        {
            var commands = _commandsManager.GetAllCommands();
            var commandList = commands.FindAll(x => string.Equals(x.Command, command.Command, StringComparison.CurrentCultureIgnoreCase));

            var num = new Random().Next(0, commandList.Count - 1);
            var c = commandList[num];
            _chatManager.SendChatMessage(c.Reply);
        }
    }
}
