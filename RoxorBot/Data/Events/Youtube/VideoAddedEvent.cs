using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Events.Youtube
{
    public class VideoAddedEvent : PubSubEvent<VideoAddedEventArgs>
    {
    }

    public class VideoAddedEventArgs
    {
        public bool IsPrimaryQueue { get; }
        public IEnumerable<YoutubeVideo> Videos { get; }

        public VideoAddedEventArgs(bool isPrimaryQueue, IEnumerable<YoutubeVideo> video)
        {
            IsPrimaryQueue = isPrimaryQueue;
            Videos = video.ToList();
        }
    }
}
