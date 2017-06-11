using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ModeratorsListReceivedEvent : PubSubEvent<List<string>>
    {
    }
}
