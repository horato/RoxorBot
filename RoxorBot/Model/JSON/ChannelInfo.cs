using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model.JSON
{
    public class ChannelInfo
    {
        public TwitchStream[] streams { get; set; }
        public int _total { get; set;}
    }
}
