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
    public class AddFilterCommandHandler : IChatCommandHandler
    {
        private readonly IFilterManager _filterManager;
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public AddFilterCommandHandler(IFilterManager filterManager, IUsersManager usersManager, IChatManager chatManager)
        {
            _filterManager = filterManager;
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.AddFilter;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || command.ArgumentsAsList.Count < 2)
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var word = command.ArgumentsAsList[0].ToLower();
            if (_filterManager.FilterExists(word))
                return;

            int value;
            if (!int.TryParse(command.ArgumentsAsList[1], out value))
                return;

            _filterManager.AddFilterWord(word, value, command.ChatMessage.DisplayName, false, false);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": the word " + word + " was successfully added to database. Reward: " + (value == -1 ? "permanent ban." : value + "s timeout."));
        }
    }
}
