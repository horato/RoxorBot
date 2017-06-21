using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Model.Database.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Interfaces.Repositories
{
    public interface IFilterRepository : IRepository<Filter>
    {
        IEnumerable<Filter> GetAll();
    }
}
