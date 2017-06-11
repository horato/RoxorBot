using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowerManager
{
    public class Follower_FollowerManager
    {
        public string created_at { get; set; }
        public Links_FollowerManager _links { get; set; }
        public bool notifications { get; set; }
        public User_FollowerManager user { get; set; }
    }
}
