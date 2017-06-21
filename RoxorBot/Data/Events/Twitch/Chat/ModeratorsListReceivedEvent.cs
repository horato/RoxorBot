using System.Collections.Generic;
using Prism.Events;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ModeratorsListReceivedEvent : PubSubEvent<List<string>>
    {
    }
}
