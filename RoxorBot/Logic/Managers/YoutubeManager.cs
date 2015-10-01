using RoxorBot.Model.Youtube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace RoxorBot
{
    class YoutubeManager
    {
        private static YoutubeManager _instance;
        private List<YoutubeVideo> Videos;
        private List<YoutubeVideo> BackupPlaylist;

        private YoutubeManager()
        {
            Logger.Log("Initializing YoutubeManager...");

            Videos = new List<YoutubeVideo>();
            BackupPlaylist = new List<YoutubeVideo>();

            new Thread(new ThreadStart(initBackupPlaylist)).Start();
        }

        private void initBackupPlaylist()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            loadBackupPlaylist("PL45A01CD33DA7756B"); //Slecna
            loadBackupPlaylist("PLHgEy2gzWJm0KYnbsuUX-PVRXsdm6sFWr"); //Slecinka

            watch.Stop();
            var elapsed = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds);
            Logger.Log("Loaded " + BackupPlaylist.Count + " songs to backup playlist in " + elapsed.Minutes + "m " + elapsed.Seconds + "s.");
        }

        public int getPlaylistCount()
        {
            return Videos.Count;
        }

        public int getBackupPlaylistCount()
        {
            return BackupPlaylist.Count;
        }

        private bool existsInPrimaryQueue(string id)
        {
            return Videos.Any(x => x.id == id);
        }

        private List<YoutubeVideo> loadBackupPlaylist(string playlistID)
        {
            List<VideoInfo> items = new List<VideoInfo>();
            string pageToken = "";
            while (pageToken != null)
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        var url = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&playlistId=" + playlistID + "&key=" + Properties.Settings.Default.youtubeKey + "&maxResults=50" + (pageToken != "" ? "&pageToken=" + pageToken : "");
                        var json = client.DownloadString(url);
                        var x = new JavaScriptSerializer().Deserialize<VideoInfoHeader>(json);

                        if (x == null)
                            break;

                        if (!string.IsNullOrWhiteSpace(x.nextPageToken))
                            pageToken = x.nextPageToken;
                        else
                            pageToken = null;

                        if (x.items != null && x.items.Length > 0)
                            items.AddRange(x.items);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
            }

            return getVideosFromVideoInfo(items);
        }

        private List<YoutubeVideo> getVideosFromVideoInfo(List<VideoInfo> items)
        {
            var result = new List<YoutubeVideo>();

            foreach (var item in items)
            {
                if (item.contentDetails == null || item.contentDetails.videoId == null)
                    continue;
                try
                {
                    lock (BackupPlaylist)
                        BackupPlaylist.Insert(new Random().Next(0, BackupPlaylist.Count), new YoutubeVideo(item.contentDetails.videoId));
                }
                catch (VideoParseException e)
                {
                    Logger.Log("Backup Playlist video load error: " + e.Message);
                }
            }
            return result;
        }

        /// <summary>
        /// may throw VideoParseException
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public YoutubeVideo addSong(string id)
        {
            if (!existsInPrimaryQueue(id))
            {
                var video = new YoutubeVideo(id);
                lock (Videos) { Videos.Add(video); }
                return video;
            }
            return null;
        }

        public void removeSong(string id)
        {
            lock (Videos)
                Videos.RemoveAll(x => x.id == id);
        }

        public YoutubeVideo getNextAndRemove()
        {
            if (Videos.Count < 1 && BackupPlaylist.Count < 1)
                return null;

            if (Videos.Count > 0)
            {
                lock (Videos)
                {
                    var temp = Videos[0];

                    Videos.RemoveAt(0);
                    return temp;
                }
            }
            else
            {
                lock (BackupPlaylist)
                {
                    var temp = BackupPlaylist[0];

                    BackupPlaylist.RemoveAt(0);
                    return temp;
                }
            }
        }

        public static YoutubeManager getInstance()
        {
            if (_instance == null)
                _instance = new YoutubeManager();

            return _instance;
        }
        
        public static string getVideoDirectLink(string id = "oHg5SJYRHA0")
        {
            var map = getFmtMap(id);
            string signature = "";

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
                var js = client.DownloadString(js_url);
                if (string.IsNullOrEmpty(js))
                    return sig;

                var reg = Regex.Match(js, "set\\(\"signature\",.*?(.*?)\\(");
                var descrambler = "";
                if (reg.Success)
                    descrambler = reg.Groups[1].Value;


                string transformations = "", rules = "";

                reg = Regex.Match(js, "var ..={(.*?)};function " + descrambler + "\\([^)]*\\){(.*?)}");
                if (reg.Success)
                {
                    transformations = reg.Groups[1].Value;
                    rules = reg.Groups[2].Value;
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
        private static Dictionary<string, string> getFmtMap(string id)
        {
            var result = new Dictionary<string, string>();
            using (WebClient client = new WebClient())
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
