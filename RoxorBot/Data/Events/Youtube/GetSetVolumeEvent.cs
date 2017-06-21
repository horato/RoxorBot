using Prism.Events;

namespace RoxorBot.Data.Events.Youtube
{
    public class GetSetVolumeEvent : PubSubEvent<GetSetVolumeEventArgs>
    {
    }

    public class GetSetVolumeEventArgs
    {
        public int NewVolume { get; set; }
        public int CurrentVolume { get; set; }
    }
}
