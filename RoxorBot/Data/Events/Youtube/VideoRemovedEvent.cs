using Prism.Events;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Events.Youtube
{
    public class VideoRemovedEvent : PubSubEvent<VideoRemovedEventArgs>
    {
    }

    public class VideoRemovedEventArgs
    {
        public bool IsPrimaryQueue { get; }
        public YoutubeVideo Video { get; }

        public VideoRemovedEventArgs(bool isPrimaryQueue, YoutubeVideo video)
        {
            IsPrimaryQueue = isPrimaryQueue;
            Video = video;
        }
    }
}
