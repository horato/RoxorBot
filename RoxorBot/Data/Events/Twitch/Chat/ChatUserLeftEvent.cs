using Prism.Events;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ChatUserLeftEvent : PubSubEvent<ChatUserLeftEventEventArgs>
    {
    }

    public class ChatUserLeftEventEventArgs
    {
        public string Name { get; }
        public bool IsModerator { get; }

        public ChatUserLeftEventEventArgs(string name, bool isModerator)
        {
            Name = name;
            IsModerator = isModerator;
        }
    }
}