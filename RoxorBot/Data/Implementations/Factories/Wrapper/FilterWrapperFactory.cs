using RoxorBot.Data.Interfaces.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Interfaces.Providers;

namespace RoxorBot.Data.Implementations.Factories
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
