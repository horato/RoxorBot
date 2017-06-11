using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using TwitchLib.Events.Client;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ChatConnectedEvent : PubSubEvent<OnConnectedArgs>
    {
    }
}
