using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using RoxorBot.Data.Implementations.Managers;
using RoxorBot.Data.Logic;

namespace RoxorBot.Data.Model.Youtube
{
    public class YoutubeVideo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string EmbedLink { get; set; }
        public string Id { get; set; }
        public TimeSpan Duration { get; set; }
        public VideoInfo Info { get; set; }
        public string Requester { get; set; }

        private YoutubeVideo()
        {

        }

        public YoutubeVideo(string videoID)
        {
            using (var client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
            {
                var json = client.DownloadData("https://www.googleapis.com/youtube/v3/videos?part=snippet,contentDetails,status&id=" + videoID + "&key=" + Properties.Settings.Default.youtubeKey);
                VideoInfoHeader info = null;
                try
                {
                    info = new DataContractJsonSerializer(typeof(VideoInfoHeader)).ReadObject(new MemoryStream(json)) as VideoInfoHeader;
                }
                catch
                {
                    throw new VideoParseException("Unknown error video: " + videoID);
                }
                if (info == null || info.Items == null || info.Items.Length < 1)
                    throw new VideoParseException("Video " + videoID + " not found");
                if (info.Items[0].ContentDetails == null)
                    throw new VideoParseException("Video " + videoID + " content details not found");
                if (info.Items[0].Snippet == null)
                    throw new VideoParseException("Video " + videoID + " info not found");
                if (info.Items[0].Status == null)
                    throw new VideoParseException("Video " + videoID + " status not found");
                /* if (!info.items[0].status.embeddable)
                    throw new VideoParseException("Video is not embedable");*/
                if (info.Items[0].Status.PrivacyStatus == "private")
                    throw new VideoParseException("Video " + videoID + " is private");

                Info = info.Items[0];
                var status = info.Items[0].Status;
                var contentDetails = info.Items[0].ContentDetails;
                var snippet = info.Items[0].Snippet;

                Duration = DurationParser.GetDuration(contentDetails.Duration);
                /*if (duration.TotalSeconds > Properties.Settings.Default.maxSongLength)
                    throw new VideoParseException("Video " + videoID + " is too long. Max length is " + Properties.Settings.Default.maxSongLength + "s.");*/
                Name = snippet.Title;
                Id = videoID;
                Address = "http://www.youtube.com/watch?v=" + videoID;
                EmbedLink = YoutubeManager.GetVideoLinkDirect(videoID);
            }
        }
    }
}
