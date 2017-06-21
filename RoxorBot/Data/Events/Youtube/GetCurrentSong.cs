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
