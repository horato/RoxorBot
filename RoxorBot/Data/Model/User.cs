using System;

namespace RoxorBot.Data.Model
{
    public class User
    {
        public string Name { get; set; }
        public string InternalName { get; set; }
        public Role Role { get; set; } = Role.Viewers;
        public bool IsOnline { get; set; }
        public int Points { get; set; }
        public bool IsFollower { get; set; }
        public DateTime? IsFollowerSince { get; set; }
        public int RewardTimer { get; set; }
        public bool IsAllowed { get; set; } 
    }
}
