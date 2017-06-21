using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace RoxorBot.Data.Implementations.Database.Repository
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
