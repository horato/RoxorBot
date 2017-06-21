using System.Collections.Generic;
using System.Linq;
using NHibernate;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Repositories
{
    public class FilterRepository : Repository<Filter>, IFilterRepository
    {
        public FilterRepository(ISessionFactory factory) : base(factory)
        {
        }

        public IEnumerable<Filter> GetAll()
        {
            return QueryAll().ToList();
        }
    }
}
