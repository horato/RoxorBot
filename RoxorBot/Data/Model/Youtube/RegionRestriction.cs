using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class RegionRestriction
    {
        [DataMember(Name = "blocked")]
        public List<string> Blocked { get; set; }
    }
}
