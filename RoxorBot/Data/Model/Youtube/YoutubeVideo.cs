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

        public YoutubeVideo(string name, string address, string embedLink, string id, TimeSpan duration, VideoInfo info, string requester)
        {
            Name = name;
            Address = address;
            EmbedLink = embedLink;
            Id = id;
            Duration = duration;
            Info = info;
            Requester = requester;
        }
    }
}
