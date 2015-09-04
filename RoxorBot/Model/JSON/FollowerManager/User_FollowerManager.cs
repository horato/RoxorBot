using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FollowerManager
{
    public class User_FollowerManager
    {
        public int _id { get; set; }
        public string name { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public Links_FollowerManager _links { get; set; }
        public string display_name { get; set; }
        public string logo { get; set; }
        public string bio { get; set; }
        public string type { get; set; }
    }
}
