using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.JSON.FollowerManager
{
    [DataContract]
    public class Follower_FollowerManager
    {
        [DataMember(Name = "created_at")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "_links")]
        public Links_FollowerManager Links { get; set; }

        [DataMember(Name = "notifications")]
        public bool Notifications { get; set; }

        [DataMember(Name = "user")]
        public User_FollowerManager User { get; set; }
    }
}
