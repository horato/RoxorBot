using TwitchLib.Models.Client;

namespace RoxorBot.Data.Interfaces.Chat
{
    public interface IChatCommandHandler
    {
        bool CanHandle(string command);
        void Handle(ChatCommand command);
    }
}
