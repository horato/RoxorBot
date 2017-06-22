using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Logic.Logging;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat
{
    public class ChatMessageHandler : IChatMessageHandler
    {
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IUsersManager _usersManager;
        private readonly IChatHandlersProvider _handlersProvider;
        private readonly IFilterManager _filterManager;
        private readonly List<IChatCommandHandler> _handlers = new List<IChatCommandHandler>();

        public ChatMessageHandler(IEventAggregator aggregator, IChatManager chatManager, IUsersManager usersManager, IFilterManager filterManager, IChatHandlersProvider handlersProvider)
        {
            _aggregator = aggregator;
            _chatManager = chatManager;
            _usersManager = usersManager;
            _handlersProvider = handlersProvider;
            _filterManager = filterManager;
        }

        public void Init()
        {
            _handlers.Clear();
            _handlers.AddRange(_handlersProvider.GetAllHandlers().ToList());
            _aggregator.GetEvent<ChatMessageReceivedEvent>().Subscribe(OnChatMessageReceivedEvent);
            _aggregator.GetEvent<ChatCommandReceivedEvent>().Subscribe(OnChatCommandReceivedEvent);
        }

        private void OnChatMessageReceivedEvent(ChatMessage msg)
        {
            if (msg == null)
                return;

            if (IsSpamMessage(msg))
            {
                PunishSpam(msg);
            }
            else
            {
                var item = _filterManager.CheckFilter(msg);
                if (item == null)
                    return;

                if (item.BanDuration == -1)
                    _chatManager.BanUser(msg.Username);
                else
                    _chatManager.TimeoutUser(msg.Username, TimeSpan.FromSeconds(item.BanDuration));

                if (Properties.Settings.Default.notifyChatRestriction)
                    _chatManager.SendChatMessage(msg.DisplayName + " awarded " + (item.BanDuration == -1 ? "permanent ban" : item.BanDuration + "s timeout") + " for filtered word HeyGuys");
                _aggregator.GetEvent<AddLogEvent>().Publish(msg.DisplayName + " awarded " + (item.BanDuration == -1 ? "permanent ban" : item.BanDuration + "s timeout") + " for filtered word.");
            }
        }

        private void OnChatCommandReceivedEvent(ChatCommand command)
        {
            if (command == null)
                return;

            var handlers = _handlers.Where(x => x.CanHandle(command.Command));
            foreach (var handler in handlers)
                handler.Handle(command);
        }

        private void PunishSpam(ChatMessage msg)
        {
            if (msg == null)
                return;

            _chatManager.TimeoutUser(msg.Username, TimeSpan.FromSeconds(120));
            if (Properties.Settings.Default.notifyChatRestriction)
                _chatManager.SendChatMessage($"Pls no spamerino {msg.Username} Keepo");
            _aggregator.GetEvent<AddLogEvent>().Publish($"{msg.DisplayName} timeouted for spamming.");
        }

        private bool IsSpamMessage(ChatMessage msg)
        {
            if (msg == null)
                return false;
            if (msg.Message.Length <= Properties.Settings.Default.maxMessageLength)
                return false;
            if (_usersManager.IsAdmin(msg.Username) || msg.IsModerator)
                return false;
            return true;
        }
    }
}
