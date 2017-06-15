using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.JSON.FollowerManager
{
    [DataContract]
    public class Links_FollowerManager
    {
        [DataMember(Name = "self")]
        public string Self { get; set; }

        [DataMember(Name = "next")]
        public string Next { get; set; }
    }
}
