using System.Runtime.Serialization;

namespace RoxorBot.Data.Model.Youtube
{
    [DataContract]
    public class PageInfo
    {
        [DataMember(Name = "totalResults")]
        public int TotalResults { get; set; }

        [DataMember(Name = "resultsPerPage")]
        public int ResultsPerPage { get; set; }
    }
}
