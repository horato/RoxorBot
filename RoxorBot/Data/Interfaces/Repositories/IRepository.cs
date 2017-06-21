using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Database
{
    public interface IRepository<T> where T : Entity
    {
        T FindById(Guid id);

        IEnumerable<T> FindByIds(IEnumerable<Guid> ids);

        void Save(T entity);

        void Remove(T entity);

        void FlushSession();
    }
}
