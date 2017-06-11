using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ChatCommandReceivedEvent : PubSubEvent<ChatCommand>
    {
    }
}
