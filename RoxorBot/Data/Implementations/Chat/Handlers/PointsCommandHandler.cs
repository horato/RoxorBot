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
    public class PointsCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IPointsManager _pointsManager;
        private readonly IUsersManager _usersManager;

        public PointsCommandHandler(IChatManager chatManager, IPointsManager pointsManager, IUsersManager usersManager)
        {
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Points;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null)
                return;

            var isAdmin = _usersManager.IsAdmin(command.ChatMessage.Username);
            if (!isAdmin || !command.ArgumentsAsList.Any())
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": You have " + _pointsManager.GetPointsForUser(command.ChatMessage.Username) + " points.");
            else
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + command.ArgumentsAsList.First() + " has " + _pointsManager.GetPointsForUser(command.ArgumentsAsList.First()) + " points.");
        }
    }
}
