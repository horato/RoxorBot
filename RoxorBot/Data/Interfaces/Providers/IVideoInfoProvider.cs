using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IVideoInfoProvider
    {
        VideoInfo Get(string videoId);
        Dictionary<string, VideoInfo> Get(IEnumerable<string> videoIds);
    }
}
