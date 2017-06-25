using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Implementations.Logging;
using RoxorBot.Data.Implementations.Managers;
using RoxorBot.Data.Interfaces.Cache;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Logic;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Implementations.Providers
{
    public class YoutubeVideoProvider : IYoutubeVideoProvider
    {
        private readonly IYoutubeVideoCache _cache;
        private readonly IVideoInfoProvider _infoProvider;

        public YoutubeVideoProvider(IYoutubeVideoCache cache, IVideoInfoProvider infoProvider)
        {
            _cache = cache;
            _infoProvider = infoProvider;
        }

        public YoutubeVideo GetVideo(string videoId)
        {
            if (_cache.Exists(videoId))
                return _cache.Get(videoId);

            return GetNew(videoId);
        }

        public Dictionary<string, YoutubeVideo> GetVideos(IEnumerable<string> videoIds)
        {
            var ret = new Dictionary<string, YoutubeVideo>();
            var requested = videoIds?.Distinct().ToList();
            if (requested == null)
                return ret;

            var infos = _infoProvider.Get(requested);
            foreach (var video in requested)
            {
                if (!infos.ContainsKey(video))
                    continue;

                var info = infos[video];
                var newVideo = CreateVideo(info);
                if (newVideo == null)
                    continue;

                ret.Add(video, newVideo);
            }

            return ret;
        }

        private YoutubeVideo GetNew(string videoId)
        {
            var info = _infoProvider.Get(videoId);
            if (info == null)
                return null;

            return CreateVideo(info);
        }

        private YoutubeVideo CreateVideo(VideoInfo info)
        {
            try
            {
                var id = info.Id;
                var address = "http://www.youtube.com/watch?v=" + id;
                var embedLink = YoutubeManager.GetVideoLinkDirect(id);
                var duration = DurationParser.GetDuration(info.ContentDetails.Duration);
                var video = new YoutubeVideo(info.Snippet.Title, address, embedLink, id, duration, info, string.Empty);

                if (!_cache.Exists(id))
                    _cache.Add(id, video);

                return video;
            }
            catch
            {
                return null;
            }
        }

        public Dictionary<string, YoutubeVideo> GetVideosFromPlaylist(string playlistId)
        {
            var ids = GetPlaylistVideoIds(playlistId);
            return GetVideos(ids);
        }

        private IEnumerable<string> GetPlaylistVideoIds(string playlistId)
        {
            var ids = new List<string>();
            var pageToken = "";
            while (pageToken != null)
            {
                try
                {
                    using (var client = new WebClient { Encoding = Encoding.UTF8 })
                    {
                        var url = CreatePlaylistRequest(playlistId, pageToken);
                        var json = client.DownloadData(url);
                        var header = new DataContractJsonSerializer(typeof(VideoInfoHeader)).ReadObject(new MemoryStream(json)) as VideoInfoHeader;
                        if (header == null)
                            break;

                        pageToken = !string.IsNullOrWhiteSpace(header.NextPageToken) ? header.NextPageToken : null;
                        if (header.Items == null)
                            continue;

                        var videoIds = header.Items.Where(x => x.ContentDetails != null).Select(x => x.ContentDetails.VideoId);
                        ids.AddRange(videoIds.Where(x => !string.IsNullOrWhiteSpace(x)));
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }

            return ids;
        }

        private string CreatePlaylistRequest(string playlistId, string pageToken = null)
        {
            var basePath = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId=";
            var apiKey = Properties.Settings.Default.youtubeKey;
            var link = $"{basePath}{playlistId}&key={apiKey}&maxResults=50";
            if (!string.IsNullOrWhiteSpace(pageToken))
                link += $"&pageToken={pageToken}";

            return link;
        }
    }
}
