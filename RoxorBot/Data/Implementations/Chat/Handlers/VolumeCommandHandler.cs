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
    public class VolumeCommandHandler : IChatCommandHandler
    {
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IUsersManager _usersManager;

        public VolumeCommandHandler(IEventAggregator aggregator, IChatManager chatManager, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _chatManager = chatManager;
            _usersManager = usersManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Volume;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null)
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var args = new GetSetVolumeEventArgs();
            var c = command.ArgumentsAsList.FirstOrDefault();
            if (c == null)
            {
                _aggregator.GetEvent<GetSetVolumeEvent>().Publish(args);
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Volume: " + args.CurrentVolume);
                return;
            }

            int volume;
            if (!int.TryParse(c, out volume))
                return;
            if (volume < 1 || volume > 100)
                return;

            args.NewVolume = volume / 100.0;
            _aggregator.GetEvent<GetSetVolumeEvent>().Publish(args);
            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Volume set to " + volume);
        }
    }
}
