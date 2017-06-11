using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Extensions;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class UptimeCommandHandler : IChatCommandHandler
    {
        private readonly IChatManager _chatManager;

        public UptimeCommandHandler(IChatManager chatManager)
        {
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Uptime;
        }

        public void Handle(ChatCommand command)
        {
            var uptime = TwitchAPI.Streams.v5.GetUptime("horato2").WaitAndReturn();
            if (uptime == null)
                return;

            var time = uptime.Value;
            _chatManager.SendChatMessage(string.Format("Streaming for " + (time.Days > 0 ? "{0}d" : "") + " {1}h {2:D2}m {3:D2}s", time.Days, time.Hours, time.Minutes, time.Seconds));
        }
    }
}
