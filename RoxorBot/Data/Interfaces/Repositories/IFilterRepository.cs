using RoxorBot.Data.Model.Database.Entities;
using System.Collections.Generic;

namespace RoxorBot.Data.Interfaces.Repositories
{
    public interface IFilterRepository : IRepository<Filter>
    {
        IEnumerable<Filter> GetAll();
    }
}
