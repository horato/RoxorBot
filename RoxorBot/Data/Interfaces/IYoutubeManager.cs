using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces
{
    public interface IYoutubeManager
    {
        int PlaylistCount { get; }
        int BackupPlaylistCount { get; }
        YoutubeVideo AddSong(string id);
        void RemoveSong(string id);
        YoutubeVideo GetNextAndRemove();
        string GetVideoDirectLink(string id = "oHg5SJYRHA0");
    }
}
