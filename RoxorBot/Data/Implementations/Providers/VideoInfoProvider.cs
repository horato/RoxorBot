using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Util;
using RoxorBot.Data.Interfaces.Cache;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Implementations.Providers
{
    public class VideoInfoProvider : IVideoInfoProvider
    {
        private readonly IVideoInfoCache _cache;

        public VideoInfoProvider(IVideoInfoCache cache)
        {
            _cache = cache;
        }

        public VideoInfo Get(string videoId)
        {
            if (_cache.Exists(videoId))
                return _cache.Get(videoId);

            var info = GetInfos(new[] { videoId });
            if (!info.Any())
                return null;

            return info.First().Value;
        }

        public Dictionary<string, VideoInfo> Get(IEnumerable<string> videoIds)
        {
            return GetInfos(videoIds);
        }

        private Dictionary<string, VideoInfo> GetInfos(IEnumerable<string> videoIds)
        {
            var ret = new Dictionary<string, VideoInfo>();
            var requested = videoIds?.Distinct().ToList();
            if (requested == null)
                return ret;

            var inCache = requested.Where(x => _cache.Exists(x)).ToList();
            foreach (var id in inCache)
            {
                var info = _cache.Get(id);
                ret.Add(id, info);
                requested.Remove(id);
            }

            if (!requested.Any())
                return ret;

            using (var client = new WebClient { Encoding = Encoding.UTF8 })
            {
                var urls = BuildRequests(requested, Properties.Settings.Default.youtubeKey);
                foreach (var url in urls)
                {
                    var json = client.DownloadData(url);
                    var header = Create(json);
                    if (header?.Items == null)
                        return ret;

                    foreach (var info in header.Items)
                    {
                        if (info?.ContentDetails == null)
                            continue;
                        if (info.Snippet == null)
                            continue;
                        if (info.Status == null)
                            continue;
                        if (info.Status.PrivacyStatus == "private")
                            continue;

                        if (!_cache.Exists(info.Id))
                            _cache.Add(info.Id, info);
                        ret.Add(info.Id, info);
                    }
                }
            }

            return ret;
        }

        private VideoInfoHeader Create(byte[] json)
        {
            return new DataContractJsonSerializer(typeof(VideoInfoHeader)).ReadObject(new MemoryStream(json)) as VideoInfoHeader;
        }

        private IEnumerable<string> BuildRequests(IEnumerable<string> ids, string apiKey)
        {
            var requested = ids?.ToList();
            if (requested == null)
                yield break;

            var split = SplitList(requested, 50);
            foreach (var part in split)
            {
                var baseUrl = "https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,status";
                var url = baseUrl + "&id=";
                var isFirst = true;
                foreach (var id in part)
                {
                    if (isFirst)
                    {
                        url += id;
                        isFirst = false;
                        continue;
                    }

                    url += $",{id}";
                }
                yield return url + $"&key={apiKey}";
            }
        }

        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (var i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }
}
