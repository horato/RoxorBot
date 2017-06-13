using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Events.Youtube;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class SkipSongCommandHandler : IChatCommandHandler
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;

        public SkipSongCommandHandler(IEventAggregator aggregator, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.SkipSong;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            _aggregator.GetEvent<SkipCurrentSongEvent>().Publish();
        }
    }
}
