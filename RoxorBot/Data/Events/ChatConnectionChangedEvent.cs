using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Enums;

namespace RoxorBot.Data.Events
{
    public class ChatConnectionChangedEvent : PubSubEvent<ChatConnectionState>
    {
    }
}
