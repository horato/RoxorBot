using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class Snippet
    {
        [DataMember(Name = "publishedAt")]
        public string PublishedAt { get; set; }

        [DataMember(Name = "channelId")]
        public string ChannelId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "thumbnails")]
        public Dictionary<string, Thumbnail> Thumbnails { get; set; }

        [DataMember(Name = "channelTitle")]
        public string ChannelTitle { get; set; }

        [DataMember(Name = "tags")]
        public string[] Tags { get; set; }

        [DataMember(Name = "categoryId")]
        public string CategoryId { get; set; }

        [DataMember(Name = "liveBroadcastContent")]
        public string LiveBroadcastContent { get; set; }

        [DataMember(Name = "playlistId")]
        public string PlaylistId { get; set; }

        [DataMember(Name = "position")]
        public int Position { get; set; }

    }
}
