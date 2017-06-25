using System.Linq;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Managers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class IsAllowedCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public IsAllowedCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.IsAllowed;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var name = command.ArgumentsAsList[0].ToLower();
            var u = _usersManager.GetUser(name);
            if (u == null)
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + name + " not found.");
            else
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + u.VisibleName + " " + (u.IsAllowed ? "is" : "is not") + " allowed");
        }
    }
}
