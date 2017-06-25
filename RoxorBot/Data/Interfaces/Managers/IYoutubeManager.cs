using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IYoutubeManager : IManagerBase
    {
        int PlaylistCount { get; }
        int BackupPlaylistCount { get; }
        YoutubeVideo AddSong(string id);
        void RemoveSong(string id);
        YoutubeVideo GetNextAndRemove();
        string GetVideoDirectLink(string id = "oHg5SJYRHA0");
    }
}
