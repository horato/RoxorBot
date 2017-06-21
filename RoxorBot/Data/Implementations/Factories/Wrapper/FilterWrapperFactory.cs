using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Implementations.Factories.Wrapper
{
    public class FilterWrapperFactory : IFilterWrapperFactory
    {
        private readonly IFilterProvider _provider;

        public FilterWrapperFactory(IFilterProvider provider)
        {
            _provider = provider;
        }

        public FilterWrapper CreateNew(Filter model)
        {
            return new FilterWrapper(model);
        }

        public FilterWrapper CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            var model = _provider.CreateNew(word, banDuration, addedBy, isRegex, isWhitelist);
            return CreateNew(model);
        }
    }
}
