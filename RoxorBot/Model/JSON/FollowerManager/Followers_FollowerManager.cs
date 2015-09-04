using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowerManager
{
    public class Followers_FollowerManager
    {
        public int _total { get; set; }
        public Links_FollowerManager _links { get; set; }
        public Follower_FollowerManager[] follows { get; set; }
    }
}
