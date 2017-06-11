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
    public class WhitelistCommandHandler : IChatCommandHandler
    {
        private readonly IFilterManager _filterManager;
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public WhitelistCommandHandler(IFilterManager filterManager, IUsersManager usersManager, IChatManager chatManager)
        {
            _filterManager = filterManager;
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Whitelist;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var word = command.ArgumentsAsList[0].ToLower();
            if (_filterManager.FilterExists(word))
                return;

            _filterManager.AddFilterWord(word, 1, command.ChatMessage.DisplayName, false, true);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": the word " + word + " is now whitelisted.");
        }
    }
}
