using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class Status
    {
        [DataMember(Name = "uploadStatus")]
        public string UploadStatus { get; set; }

        [DataMember(Name = "privacyStatus")]
        public string PrivacyStatus { get; set; }

        [DataMember(Name = "license")]
        public string License { get; set; }

        [DataMember(Name = "embeddable")]
        public bool Embeddable { get; set; }

        [DataMember(Name = "publicStatsViewable")]
        public bool PublicStatsViewable { get; set; }
    }
}
