using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
{
    public class FilterItem
    {
        public string word { get; set; }
        public string duration { get; set; }
        public string addedBy { get; set; }
        public bool isRegex { get; set; }
        public bool isWhitelist { get; set; }
    }
}
