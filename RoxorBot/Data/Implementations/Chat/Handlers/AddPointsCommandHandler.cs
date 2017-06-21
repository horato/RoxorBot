using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class AddPointsCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IPointsManager _pointsManager;
        private readonly IUsersManager _usersManager;

        public AddPointsCommandHandler(IChatManager chatManager, IPointsManager pointsManager, IUsersManager usersManager)
        {
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.AddPoints;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || command.ArgumentsAsList.Count < 2)
                return;
            if (!_usersManager.IsSuperAdmin(command.ChatMessage.Username))
                return;

            var name = command.ArgumentsAsList[0].ToLower();
            int value;
            if (!int.TryParse(command.ArgumentsAsList[1], out value))
                return;

            _pointsManager.AddPoints(name, value);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Added " + value + " points to " + name + ".");
        }
    }
}
