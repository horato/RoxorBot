using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model.Youtube
{
    public class Status
    {
        public string uploadStatus { get; set; }
        public string privacyStatus { get; set; }
        public string license { get; set; }
        public bool embeddable { get; set; }
        public bool publicStatsViewable { get; set; }
    }
}
