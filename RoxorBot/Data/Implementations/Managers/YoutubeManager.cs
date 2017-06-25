using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Youtube;
using RoxorBot.Data.Implementations.Logging;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Implementations.Managers
{
    public class YoutubeManager : IYoutubeManager
    {
        private readonly ILogger _logger = LoggerProvider.GetLogger();
        private readonly IEventAggregator _aggregator;
        private readonly List<YoutubeVideo> _videos = new List<YoutubeVideo>();
        private readonly List<YoutubeVideo> _backupPlaylist = new List<YoutubeVideo>();
        private bool _isPlaylistLoading = false;
        public int PlaylistCount => _videos.Count;
        public int BackupPlaylistCount => _backupPlaylist.Count;

        public YoutubeManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        public void Init()
        {
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing YoutubeManager...");
            InitBackupPlaylist();
        }

        private void InitBackupPlaylist()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.youtubeKey))
                return;

            _isPlaylistLoading = true;
            var watch = System.Diagnostics.Stopwatch.StartNew();

            LoadBackupPlaylist("PL45A01CD33DA7756B"); //Slecna
            LoadBackupPlaylist("PLHgEy2gzWJm0KYnbsuUX-PVRXsdm6sFWr"); //Slecinka

            watch.Stop();
            var elapsed = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + _backupPlaylist.Count + " songs to backup playlist in " + elapsed.Minutes + "m " + elapsed.Seconds + "s.");
            _isPlaylistLoading = false;
        }

        private bool ExistsInPrimaryQueue(string id)
        {
            return _videos.Any(x => x.Id == id);
        }

        private void LoadBackupPlaylist(string playlistID)
        {
            var items = new List<VideoInfo>();
            var pageToken = "";
            while (pageToken != null)
            {
                try
                {
                    using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                    {
                        var url =
                            "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId=" +
                            playlistID + "&key=" + Properties.Settings.Default.youtubeKey + "&maxResults=50" +
                            (pageToken != "" ? "&pageToken=" + pageToken : "");
                        var json = client.DownloadData(url);
                        var x = new DataContractJsonSerializer(typeof(VideoInfoHeader)).ReadObject(new MemoryStream(json)) as VideoInfoHeader;
                        if (x == null)
                            break;

                        if (!string.IsNullOrWhiteSpace(x.NextPageToken))
                            pageToken = x.NextPageToken;
                        else
                            pageToken = null;

                        if (x.Items != null && x.Items.Length > 0)
                            items.AddRange(x.Items);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }
            GetVideosFromVideoInfo(items);
        }

        private void GetVideosFromVideoInfo(IEnumerable<VideoInfo> items)
        {
            foreach (var item in items)
            {
                if (item.ContentDetails?.VideoId == null)
                    continue;
                try
                {
                    var video = new YoutubeVideo(item.ContentDetails.VideoId);
                    lock (_backupPlaylist)
                        _backupPlaylist.Insert(new Random().Next(0, _backupPlaylist.Count), video);

                    _aggregator.GetEvent<VideoAddedEvent>().Publish(new VideoAddedEventArgs(false, video));
                }
                catch (VideoParseException e)
                {
                    _logger.Info("Backup Playlist video load error: " + e.Message);
                }
            }
        }

        /// <summary>
        /// may throw VideoParseException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public YoutubeVideo AddSong(string id)
        {
            try
            {
                if (ExistsInPrimaryQueue(id))
                    return null;

                var video = new YoutubeVideo(id);
                lock (_videos)
                    _videos.Add(video);

                _aggregator.GetEvent<VideoAddedEvent>().Publish(new VideoAddedEventArgs(true, video));
                return video;
            }
            catch
            {
                //
            }

            return null;
        }

        public void RemoveSong(string id)
        {
            lock (_videos)
            {
                var video = _videos.FirstOrDefault(x => x.Id == id);
                if (video == null)
                    return;

                _videos.RemoveAll(x => x.Id == id);
                _aggregator.GetEvent<VideoRemovedEvent>().Publish(new VideoRemovedEventArgs(true, video));
            }
        }

        public YoutubeVideo GetNextAndRemove()
        {
            if (_backupPlaylist.Count < 5 && !_isPlaylistLoading)
                new Thread(InitBackupPlaylist).Start();

            if (_videos.Count > 0)
            {
                lock (_videos)
                {
                    var temp = _videos.FirstOrDefault();
                    if (temp == null)
                        return null;

                    _videos.Remove(temp);
                    _aggregator.GetEvent<VideoRemovedEvent>().Publish(new VideoRemovedEventArgs(true, temp));
                    return temp;
                }
            }

            lock (_backupPlaylist)
            {
                var temp = _backupPlaylist.FirstOrDefault();
                if (temp == null)
                    return null;

                _backupPlaylist.Remove(temp);
                _aggregator.GetEvent<VideoRemovedEvent>().Publish(new VideoRemovedEventArgs(false, temp));
                return temp;
            }
        }

        public static string GetVideoLinkDirect(string id = "oHg5SJYRHA0")
        {
            var map = GetFmtMap(id);
            var signature = "";

            if (map.ContainsKey("sig"))
            {
                signature = "&signature=" + map["sig"];
            }
            else if (map.ContainsKey("s"))
            {
                if (map.ContainsKey("js"))
                {
                    signature = "&signature=" + js_descramble(map["s"], map["js"]);
                }
            }
            if (map.ContainsKey("url"))
            {
                return map["url"] + signature;
            }

            return "";
        }

        public string GetVideoDirectLink(string id = "oHg5SJYRHA0")
        {
            return GetVideoLinkDirect(id);
        }

        /// <summary>
        /// Thanks to VLC media player for their youtube lua script
        /// </summary>
        /// <param name="sig"></param>
        /// <param name="js_url"></param>
        /// <returns></returns>
        private static string js_descramble(string sig, string js_url)
        {
            using (var client = new WebClient())
            {
                if (js_url.StartsWith("/"))
                    js_url = "https://www.youtube.com" + js_url;

                var js = client.DownloadString(js_url);
                if (string.IsNullOrEmpty(js))
                    return sig;

                var reg = Regex.Match(js, "set\\(\"signature\",.*?(.*?)\\(");
                var descrambler = "";
                if (reg.Success)
                    descrambler = reg.Groups[1].Value;


                string transformations = "", rules = "", helper = "";

                //reg = Regex.Match(js, "var ..={(.*?)};function " + descrambler + "\\([^)]*\\){(.*?)}");
                reg = Regex.Match(js, "^" + descrambler + "=function\\([^)]*\\){(.*)};", RegexOptions.Multiline);
                if (reg.Success)
                {
                    rules = reg.Groups[1].Value;
                }

                reg = Regex.Match(rules, ";(..)\\...\\(");
                if (reg.Success)
                {
                    helper = reg.Groups[1].Value;
                }

                reg = Regex.Match(js, helper + "={(.*?)};", RegexOptions.Singleline);
                if (reg.Success)
                {
                    transformations = reg.Groups[1].Value;
                }

                var trans = new Dictionary<string, string>();
                reg = Regex.Match(transformations, "(..):function\\([^)]*\\){([^}]*)}");
                while (reg.Success)
                {
                    var meth = reg.Groups[1].Value;
                    var code = reg.Groups[2].Value;

                    if (Regex.IsMatch(code, "\\.reverse\\("))
                        trans.Add(meth, "reverse");
                    else if (Regex.IsMatch(code, "\\.splice\\("))
                        trans.Add(meth, "slice");
                    else if (Regex.IsMatch(code, "var c="))
                        trans.Add(meth, "swap");
                    else
                        System.Diagnostics.Debug.WriteLine("Couldn't parse unknown youtube video URL signature transformation");

                    reg = reg.NextMatch();
                }

                var missing = false;
                reg = Regex.Match(rules, "..\\.(..)\\([^,]+,(\\d+)\\)");
                while (reg.Success)
                {
                    var meth = reg.Groups[1].Value;
                    var idx = int.Parse(reg.Groups[2].Value);

                    if (trans[meth] == "reverse")
                    {
                        char[] charArray = sig.ToCharArray();
                        Array.Reverse(charArray);
                        sig = new string(charArray);
                    }
                    else if (trans[meth] == "slice")
                    {
                        sig = sig.Substring(idx);
                    }
                    else if (trans[meth] == "swap")
                    {
                        if (idx > 1)
                        {
                            string replicate = "";
                            for (int i = 1; i < idx; i++)
                                replicate += ".";
                            //signature = string.gsub( sig, "^(.)("..string.rep( ".", idx - 1 )..")(.)(.*)$", "%3%2%1%4" )
                            var reg2 = Regex.Match(sig, "^(.)(" + replicate + ")(.)(.*)$");
                            if (reg2.Success)
                            {
                                var first = reg2.Groups[1].Value;
                                var second = reg2.Groups[2].Value;
                                var third = reg2.Groups[3].Value;
                                var fourth = reg2.Groups[4].Value;
                                sig = third + second + first + fourth;
                            }
                            else
                                throw new Exception("Failed to reverse 2");
                        }
                        else if (idx == 1)
                        {
                            var reg2 = Regex.Match(sig, "^(.)(.)");
                            if (reg2.Success)
                            {
                                var first = reg2.Groups[1].Value;
                                var second = reg2.Groups[2].Value;
                                sig = Regex.Replace(sig, "^" + first + second, second + first);
                            }
                            else
                                throw new Exception("Failed to reverse");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Couldn't apply unknown youtube video URL signature transformation");
                            missing = true;
                        }
                    }
                    reg = reg.NextMatch();
                }


                if (missing)
                    System.Diagnostics.Debug.WriteLine("Couldn't process youtube video URL, please check for updates to this script");

                return sig;
            }
        }

        /// <summary>
        /// Thanks to VLC media player for their youtube lua script
        /// </summary>
        /// <param name="sig"></param>
        /// <param name="js_url"></param>
        /// <returns></returns>
        private static Dictionary<string, string> GetFmtMap(string id)
        {
            var result = new Dictionary<string, string>();
            using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
            {
                var info = client.DownloadString("http://www.youtube.com/watch?v=" + id);
                var qualityIndex = 0;
                var regex = Regex.Match(info, "\"fmt_list\":\"(.*?)\"");
                if (regex.Success)
                {
                    var map = regex.Groups[1].Value.Replace("\\/", "/").Split(',');
                    for (int i = 0; i < map.Length; i++)
                    {
                        if (map[i].Contains("x360"))
                        {
                            qualityIndex = i;
                            break;
                        }
                    }
                }

                regex = Regex.Match(info, "\"url_encoded_fmt_stream_map\":\"(.*?)\"");
                if (regex.Success)
                {
                    var map = regex.Groups[1].Value.Replace("\\u0026", "&").Split(',');
                    var m = map[qualityIndex];

                    var values = m.Split('&');
                    foreach (var kvp in values)
                    {
                        var str = kvp.Split('=');
                        result.Add(str[0], HttpUtility.UrlDecode(str[1]));
                    }
                }

                regex = Regex.Match(info, "\"js\":\"(.*?)\"");
                if (regex.Success)
                {
                    var js_url = HttpUtility.UrlDecode(regex.Groups[1].Value);
                    js_url = js_url.Replace("\\/", "/");
                    js_url = Regex.Replace(js_url, "^//", "http://");
                    result.Add("js", js_url);
                }
            }
            return result;
        }
    }
}
