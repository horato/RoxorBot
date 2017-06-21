using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Model.Database.Entities
{
    public class Filter : Entity
    {
        public virtual string Word { get; set; }
        public virtual int BanDuration { get; set; }
        public virtual string Author { get; set; }
        public virtual bool IsRegex { get; set; }
        public virtual bool IsWhitelist { get; set; }

        //nhibernate
        public Filter()
        {

        }

        public Filter(string word, int banDuration, string author, bool isRegex, bool isWhitelist)
        {
            Word = word;
            BanDuration = banDuration;
            Author = author;
            IsRegex = isRegex;
            IsWhitelist = isWhitelist;
        }

        public Filter(Guid id, string word, int banDuration, string author, bool isRegex, bool isWhitelist) : this(word, banDuration, author, isRegex, isWhitelist)
        {
            Id = id;
        }
    }
}
