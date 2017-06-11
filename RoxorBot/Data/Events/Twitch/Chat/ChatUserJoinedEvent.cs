using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Model;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ChatUserJoinedEvent : PubSubEvent<ChatUserJoinedEventArgs>
    {
    }

    public class ChatUserJoinedEventArgs
    {
        public string Name { get; }
        public bool IsModerator { get; }

        public ChatUserJoinedEventArgs(string name, bool isModerator)
        {
            Name = name;
            IsModerator = isModerator;
        }
    }
}
