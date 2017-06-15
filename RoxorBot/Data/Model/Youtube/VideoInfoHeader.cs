using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class VideoInfoHeader
    {
        [DataMember(Name = "kind")]
        public string Kind { get; set; }

        [DataMember(Name = "etag")]
        public string Etag { get; set; }

        [DataMember(Name = "nextPageToken")]
        public string NextPageToken { get; set; }

        [DataMember(Name = "pageInfo")]
        public PageInfo PageInfo { get; set; }

        [DataMember(Name = "items")]
        public VideoInfo[] Items { get; set; }
    }
}
