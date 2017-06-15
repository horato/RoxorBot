using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.JSON.FollowerManager
{
    [DataContract]
    public class Followers_FollowerManager
    {
        [DataMember(Name = "_total")]
        public int Total { get; set; }

        [DataMember(Name = "_links")]
        public Links_FollowerManager Links { get; set; }

        [DataMember(Name = "follows")]
        public Follower_FollowerManager[] Follows { get; set; }
    }
}
