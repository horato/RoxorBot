using System.Linq;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Managers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class NotifyNextSongCommandHandler : IChatCommandHandler
    {
        private readonly IUsersManager _usersManager;
        private readonly IChatManager _chatManager;

        public NotifyNextSongCommandHandler(IUsersManager usersManager, IChatManager chatManager)
        {
            _usersManager = usersManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.NotifyNextSong;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;
            if (!_usersManager.IsAdmin(command.ChatMessage.Username))
                return;

            var c = command.ArgumentsAsList.First().ToLower();
            if (!(c == "on" || c == "off"))
                return;

            Properties.Settings.Default.notifyCurrentPlayingSong = c == "on";
            Properties.Settings.Default.Save();

            _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": next song notification is now " + command);
        }
    }
}
