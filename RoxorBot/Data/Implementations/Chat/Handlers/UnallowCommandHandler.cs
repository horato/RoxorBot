using System.Linq;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class UnallowCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public UnallowCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Unallow;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsSuperAdmin(command.ChatMessage.Username))
                return;

            var name = command.ArgumentsAsList[0].ToLower();
            var user = _usersManager.GetUser(name);
            if (user == null)
                return;

            _usersManager.RevokeAllowUser(name);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Successfully revoked allow from " + user.Name + ".");
        }
    }
}
