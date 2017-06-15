using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Model.Youtube;

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
