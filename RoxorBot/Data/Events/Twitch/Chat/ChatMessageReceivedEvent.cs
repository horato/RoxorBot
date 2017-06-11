using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Events
{
    public class ChatMessageReceivedEvent : PubSubEvent<ChatMessage>
    {
    }
}
