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
    public class GetTimerCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public GetTimerCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.GetTimer;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ArgumentsAsList == null)
                return;
            if (command.ChatMessage == null)
                return;
            if (!command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsSuperAdmin(command.ChatMessage.Username))
                return;

            var name = command.ArgumentsAsList.FirstOrDefault();
            var u = _usersManager.GetUser(name);
            if (u == null)
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + name + " not found.");
            else
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + u.Name + " has " + u.RewardTimer + " reward timer out of 30.");
        }
    }
}
