using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
{
    class AutomatedMessage
    {
        public string message { get; set; }
        public int interval { get; set; }
        public System.Timers.Timer timer { get; set; }
    }
}
