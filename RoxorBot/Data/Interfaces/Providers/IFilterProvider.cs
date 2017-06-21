using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IFilterProvider
    {
        Filter GetAutomatedMessage(Guid id);
        Filter CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
    }
}
