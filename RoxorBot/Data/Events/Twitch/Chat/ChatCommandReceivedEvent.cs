using Prism.Events;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Events.Twitch.Chat
{
    public class ChatCommandReceivedEvent : PubSubEvent<ChatCommand>
    {
    }
}
