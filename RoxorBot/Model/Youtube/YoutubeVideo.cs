using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace RoxorBot.Model.Youtube
{
    public class YoutubeVideo
    {
        public string name { get; set; }
        public string address { get; set; }
        public string embedLink { get; set; }
        public string id { get; set; }
        public TimeSpan duration { get; set; }
        public VideoInfo info { get; set; }
        public string requester { get; set; }

        private YoutubeVideo()
        {

        }

        public YoutubeVideo(string videoID)
        {
            using (var client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
            {
                string json = client.DownloadString("https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,status&id=" + videoID + "&key=" + Properties.Settings.Default.youtubeKey);
                VideoInfoHeader info = null;
                try
                {
                    info = new JavaScriptSerializer().Deserialize<VideoInfoHeader>(json);
                }
                catch { throw new VideoParseException("Unknown error video: " + videoID); }
                if (info == null || info.items == null || info.items.Length < 1)
                    throw new VideoParseException("Video " + videoID + " not found");
                if (info.items[0].contentDetails == null)
                    throw new VideoParseException("Video " + videoID + " content details not found");
                if (info.items[0].snippet == null)
                    throw new VideoParseException("Video " + videoID + " info not found");
                if (info.items[0].status == null)
                    throw new VideoParseException("Video " + videoID + " status not found");
                /* if (!info.items[0].status.embeddable)
                    throw new VideoParseException("Video is not embedable");*/
                if (info.items[0].status.privacyStatus == "private")
                    throw new VideoParseException("Video " + videoID + " is private");

                this.info = info.items[0];
                var status = info.items[0].status;
                var contentDetails = info.items[0].contentDetails;
                var snippet = info.items[0].snippet;

                duration = DurationParser.GetDuration(contentDetails.duration);
                /*if (duration.TotalSeconds > Properties.Settings.Default.maxSongLength)
                    throw new VideoParseException("Video " + videoID + " is too long. Max length is " + Properties.Settings.Default.maxSongLength + "s.");*/
                name = snippet.title;
                id = videoID;
                address = "http://www.youtube.com/watch?v=" + videoID;
                embedLink = YoutubeManager.getVideoDirectLink(videoID);
            }
        }
    }
}
