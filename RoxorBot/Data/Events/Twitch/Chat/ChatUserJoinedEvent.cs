using Prism.Events;

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
