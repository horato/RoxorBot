using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Cache;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Implementations.Cache
{
    public class YoutubeVideoCache : CacheBase<YoutubeVideo, string>, IYoutubeVideoCache
    {

    }
}
