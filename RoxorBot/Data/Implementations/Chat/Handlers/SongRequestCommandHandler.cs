using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Interfaces.Chat;
using TwitchLib.Models.Client;
using System.Text.RegularExpressions;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations.Chat.Handlers
{
    public class SongRequestCommandHandler : IChatCommandHandler
    {
        private readonly IYoutubeManager _youtubeManager;
        private readonly IChatManager _chatManager;

        public SongRequestCommandHandler(IYoutubeManager youtubeManager, IChatManager chatManager)
        {
            _youtubeManager = youtubeManager;
            _chatManager = chatManager;
        }

        public bool CanHandle(string command)
        {
            return command == ChatCommands.SongRequest;
        }

        public void Handle(ChatCommand command)
        {
            if (command?.ChatMessage == null)
                return;
            if (command.ArgumentsAsList == null || !command.ArgumentsAsList.Any())
                return;

            var link = command.ArgumentsAsList.First();
            var match = Regex.Match(link, "youtu(?:\\.be|be\\.com)\\/(?:.*v(?:\\/|=)|(?:.*\\/)?)([a-zA-Z0-9-_]+)");
            if (!match.Success)
                return;

            try
            {
                var id = match.Groups[1].Value;
                var video = _youtubeManager.AddSong(id);
                if (video == null)
                    return;

                if (video.Duration.TotalSeconds > Properties.Settings.Default.maxSongLength)
                {
                    _youtubeManager.RemoveSong(video.Id);
                    _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": Video " + id + " is too long. Max length is " + Properties.Settings.Default.maxSongLength + "s.");
                    return;
                }

                video.Requester = command.ChatMessage.DisplayName;
                _chatManager.SendChatMessage(command.ChatMessage.DisplayName + ": " + video.Info.Snippet.Title + " added to queue.");
            }
            catch
            {
               // ignored
            }
        }
    }
}
