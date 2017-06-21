using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class SinceCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public SinceCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.Since;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;

            var user = _usersManager.GetUser(command.ChatMessage.Username);
            if (user == null || !user.IsFollower || user.IsFollowerSince == null)
                return;

            var time = user.IsFollowerSince.Value;
            _chatManager.SendChatMessage($"{command.ChatMessage.DisplayName} is following since {time.Day}.{time.Month:D2}.{time.Year} {time.Hour}:{time.Minute:D2}:{time.Second:D2}");
        }
    }
}
