using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Events.Youtube
{
    public class GetCurrentSongEvent : PubSubEvent<GetCurrentSongEventArgs>
    {

    }

    public class GetCurrentSongEventArgs
    {
        public YoutubeVideo Video { get; set; }
    }
}
