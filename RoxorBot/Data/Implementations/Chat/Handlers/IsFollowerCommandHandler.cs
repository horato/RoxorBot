using System.Linq;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Managers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class IsFollowerCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public IsFollowerCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.IsFollower;
        }

        public void Handle(ChatCommand command)
        {
            if (command.ChatMessage == null)
                return;
            if (command?.ArgumentsAsList == null)
                return;
            if (!command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var name = command.ArgumentsAsList.FirstOrDefault();
            var u = _usersManager.GetUser(name);
            if (u == null)
                return;

            if (u.IsFollower)
                _chatManager.SendChatMessage(u.VisibleName + " is following.");
            else
                _chatManager.SendChatMessage(u.VisibleName + "is not following.");
        }
    }
}
