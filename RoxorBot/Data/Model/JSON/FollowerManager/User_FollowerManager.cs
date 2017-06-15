using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.JSON.FollowerManager
{
    [DataContract]
    public class User_FollowerManager
    {
        [DataMember(Name = "_id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "created_at")]
        public string CreatedAt { get; set; }

        [DataMember(Name = "updated_at")]
        public string UpdatedAt { get; set; }

        [DataMember(Name = "_links")]
        public Links_FollowerManager Links { get; set; }

        [DataMember(Name = "display_name")]
        public string DisplayName { get; set; }

        [DataMember(Name = "logo")]
        public string Logo { get; set; }

        [DataMember(Name = "bio")]
        public string Bio { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
    }
}
