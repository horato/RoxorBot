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

        private YoutubeVideo()
        {

        }

        public YoutubeVideo(string videoID)
        {
            using (var client = new WebClient())
            {
                string json = client.DownloadString("https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,status&id=" + videoID + "&key=" + Properties.Settings.Default.youtubeKey);
                var info = new JavaScriptSerializer().Deserialize<VideoInfoHeader>(json);

                if (info == null || info.items == null || info.items.Length < 1)
                    throw new VideoParseException("Video " + videoID + " not found");
                if (info.items[0].contentDetails == null)
                    throw new VideoParseException("Video " + videoID + " content details not found");
                if (info.items[0].snippet == null)
                    throw new VideoParseException("Video " + videoID + " info not found");
                if (info.items[0].status == null)
                    throw new VideoParseException("Video " + videoID + " status not found");

                this.info = info.items[0];
                var status = info.items[0].status;
                /* if (!status.embeddable)
                     throw new VideoParseException("Video is not embedable");*/
                if (status.privacyStatus == "private")
                    throw new VideoParseException("Video " + videoID + " is private");
                var contentDetails = info.items[0].contentDetails;
                var snippet = info.items[0].snippet;

                duration = DurationParser.GetDuration(contentDetails.duration);
                name = snippet.title;
                id = videoID;
                address = "http://www.youtube.com/watch?v=" + videoID;
                embedLink = YoutubeManager.getVideoDirectLink(videoID);
            }
        }
    }
}
