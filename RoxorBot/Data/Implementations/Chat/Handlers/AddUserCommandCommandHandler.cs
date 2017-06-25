using System.Linq;
using System.Text;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Managers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class AddUserCommandCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IUserCommandsManager _userCommandsManager;
        private readonly IUsersManager _usersManager;

        public AddUserCommandCommandHandler(IChatManager chatManager, IUserCommandsManager commandsManager, IUsersManager usersManager)
        {
            _chatManager = chatManager;
            _userCommandsManager = commandsManager;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.AddComm;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || command.ArgumentsAsList.Count < 2)
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var sb = new StringBuilder();
            var c = command.ArgumentsAsList.First();
            for (var i = 1; i < command.ArgumentsAsList.Count; i++)
                sb.Append(command.ArgumentsAsList[i]);
            var reply = sb.ToString();

            _userCommandsManager.AddCommand(c, reply);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Command " + c + " added.");
        }
    }
}
