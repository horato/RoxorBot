using Prism.Events;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Events.Youtube;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class SongCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;
        private readonly IEventAggregator _aggregator;

        public SongCommandHandler(IChatManager chatManager, IEventAggregator aggregator)
        {
            _chatManager = chatManager;
            _aggregator = aggregator;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Song;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;

            var args = new GetCurrentSongEventArgs();
            _aggregator.GetEvent<GetCurrentSongEvent>().Publish(args);
            if (args.Video == null)
                return;

            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + args.Video.Name);
        }
    }
}
