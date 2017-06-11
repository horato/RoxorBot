using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
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
