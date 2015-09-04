using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
{
    class User
    {
        public string Name { get; set; }

        public string InternalName { get; set; }

        public Role Role { get; set; }

        public bool isOnline { get; set; }

        public int Points { get; set; }

        public bool IsFollower { get; set; }

        [System.ComponentModel.DefaultValue(0)]
        public int RewardTimer { get; set; } 
    }
}
