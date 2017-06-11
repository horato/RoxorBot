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
    public class RemovePointsCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IPointsManager _pointsManager;
        private readonly IUsersManager _usersManager;

        public RemovePointsCommandHandler(IChatManager chatManager, IPointsManager pointsManager, IUsersManager usersManager)
        {
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;
        }
        public bool CanHandle(string command)
        {
            return command == ChatCommands.RemovePoints;
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

            _pointsManager.RemovePoints(name, value);
            _chatManager.SendChatMessage(command.ChatMessage.Username + ": Subtracted " + value + " points from " + name + ". " + name + " now has " + _pointsManager.GetPointsForUser(name) + " points.");
        }
    }
}
