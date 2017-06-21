using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Factories.Wrapper
{
    public interface IFilterWrapperFactory
    {
        FilterWrapper CreateNew(Filter model);
        FilterWrapper CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
    }
}
