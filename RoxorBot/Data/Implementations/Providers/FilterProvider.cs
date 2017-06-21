using RoxorBot.Data.Interfaces.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Interfaces.Repositories;

namespace RoxorBot.Data.Implementations.Providers
{
    public class FilterProvider : IFilterProvider
    {
        private readonly IFilterRepository _repository;

        public FilterProvider(IFilterRepository repository)
        {
            _repository = repository;
        }

        public Filter CreateNew(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            var filter = new Filter(word, banDuration, addedBy, isRegex, isWhitelist);
            _repository.Save(filter);
            _repository.FlushSession();
            return filter;
        }

        public Filter GetAutomatedMessage(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _repository.FindById(id);
        }
    }
}
