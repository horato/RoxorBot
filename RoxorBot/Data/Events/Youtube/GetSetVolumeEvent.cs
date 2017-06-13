using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;

namespace RoxorBot.Data.Events.Youtube
{
    public class GetSetVolumeEvent : PubSubEvent<GetSetVolumeEventArgs>
    {
    }

    public class GetSetVolumeEventArgs
    {
        public double NewVolume { get; set; }
        public double CurrentVolume { get; set; }
    }
}
