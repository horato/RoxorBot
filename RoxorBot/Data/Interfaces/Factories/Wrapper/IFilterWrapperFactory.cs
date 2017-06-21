using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories
{
    public interface IFilterWrapperFactory
    {
        FilterWrapper CreateNew(Filter model);
        FilterWrapper CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
    }
}
