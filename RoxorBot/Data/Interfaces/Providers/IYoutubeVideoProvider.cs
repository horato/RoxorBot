using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IYoutubeVideoProvider
    {
        YoutubeVideo GetVideo(string videoId);
        Dictionary<string, YoutubeVideo> GetVideos(IEnumerable<string> videoIds);
        Dictionary<string, YoutubeVideo> GetVideosFromPlaylist(string playlistId);
    }
}
