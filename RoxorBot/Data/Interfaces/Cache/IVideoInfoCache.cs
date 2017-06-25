using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Youtube;

namespace RoxorBot.Data.Interfaces.Cache
{
    public interface IVideoInfoCache : ICacheBase<VideoInfo, string>
    {
    }
}
