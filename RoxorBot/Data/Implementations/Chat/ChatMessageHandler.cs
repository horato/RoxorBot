using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Model;
using RoxorBot.Model.JSON;
using TwitchLib.Models.Client;
using System.Text.RegularExpressions;

namespace RoxorBot.Data.Implementations
{
    public class ChatMessageHandler : IChatMessageHandler
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IUsersManager _usersManager;
        private readonly IChatHandlersProvider _handlersProvider;
        private readonly IFilterManager _filterManager;
        private readonly List<IChatCommandHandler> _handlers = new List<IChatCommandHandler>();

        public ChatMessageHandler(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IUsersManager usersManager, IFilterManager filterManager, IChatHandlersProvider handlersProvider)
        {
            _logger = logger;
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
                PunishSpam(msg);
            else if (_filterManager.CheckFilter(msg))
            {
                var item = _filterManager.GetFilter(msg.Message);
                if (item == null)
                {
                    var temp = _filterManager.GetAllFilters(FilterMode.Regex);
                    foreach (var filter in temp)
                        if (Regex.IsMatch(msg.Message, filter.word))
                            item = filter;
                }

                if (item == null)
                    return;
                if (item.duration == -1)
                    _chatManager.BanUser(msg.Username);
                else
                    _chatManager.TimeoutUser(msg.Username, TimeSpan.FromSeconds(item.duration));

                if (Properties.Settings.Default.notifyChatRestriction)
                    _chatManager.SendChatMessage(msg.DisplayName + " awarded " + (item.duration == -1 ? "permanent ban" : item.duration + "s timeout") + " for filtered word HeyGuys");
                _aggregator.GetEvent<AddLogEvent>().Publish(msg.DisplayName + " awarded " + (item.duration == -1 ? "permanent ban" : item.duration + "s timeout") + " for filtered word.");
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
                _chatManager.SendChatMessage("Pls no spamerino " + msg.Username + " Keepo");
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
