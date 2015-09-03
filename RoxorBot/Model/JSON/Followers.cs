using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model.JSON
{
    public class Followers
    {
        public string created_at { get; set; }
        public Links _links { get; set; }
        public bool notifications { get; set; }
        public object channel { get; set; }
        public string error { get; set; }
        public int status { get; set; }
        public string message { get; set; }
    }
}
