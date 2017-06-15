using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class ContentDetails
    {
        [DataMember(Name = "duration")]
        public string Duration { get; set; }

        [DataMember(Name = "dimension")]
        public string Dimension { get; set; }

        [DataMember(Name = "definition")]
        public string Definition { get; set; }

        [DataMember(Name = "caption")]
        public string Caption { get; set; }

        [DataMember(Name = "licensedContent")]
        public bool LicensedContent { get; set; }

        [DataMember(Name = "regionRestriction")]
        public RegionRestriction RegionRestriction { get; set; }

        [DataMember(Name = "videoId")]
        public string VideoId { get; set; }
    }
}
