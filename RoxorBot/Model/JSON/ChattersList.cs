using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model.JSON
{
    public class ChattersList
    {
        public string[] moderators { get; set; }
        public string[] staff { get; set; }
        public string[] admins { get; set; }
        public string[] global_mods { get; set; }
        public string[] viewers { get; set; }
    }
}
