using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class VideoInfo
    {
        [DataMember(Name = "kind")]
        public string Kind { get; set; }

        [DataMember(Name = "etag")]
        public string Etag { get; set; }

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "snippet")]
        public Snippet Snippet { get; set; }

        [DataMember(Name = "contentDetails")]
        public ContentDetails ContentDetails { get; set; }

        [DataMember(Name = "status")]
        public Status Status { get; set; }

    }
}
