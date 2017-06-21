using System;
using System.Collections.Generic;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Repositories
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
