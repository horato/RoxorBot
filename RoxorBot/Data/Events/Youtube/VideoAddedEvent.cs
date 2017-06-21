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
        public YoutubeVideo Video { get; }

        public VideoAddedEventArgs(bool isPrimaryQueue, YoutubeVideo video)
        {
            IsPrimaryQueue = isPrimaryQueue;
            Video = video;
        }
    }
}
