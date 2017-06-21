using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate;
using NHibernate.Linq;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Repositories
{
    public abstract class Repository<T> : IRepository<T> where T : Entity
    {
        private ISession _session;

        public Repository(ISessionFactory factory)
        {
            _session = factory.OpenSession();
        }

        ~Repository()
        {
            _session.Close();
            _session.Dispose();
            _session = null;
        }

        public T FindById(Guid id)
        {
            var entity = _session.Get<T>(id);

            if (entity == null)
            {
                throw new InvalidOperationException(string.Format("Entita typu:{0} s Id:{1} nebyla nalezena.", typeof(T).Name, id));
            }

            return entity;
        }

        public IEnumerable<T> FindByIds(IEnumerable<Guid> ids)
        {
            var foundEntities = FindByCriteria(x => ids.Contains(x.Id));

            if (ids.Count() != foundEntities.Count())
                throw new InvalidOperationException($"Pocet nalezenych entit({foundEntities.Count()}) typu {typeof(T).Name} neodpovida poctu pozadovanych entit({ids.Count()}) dle zadanych Id.");

            return foundEntities;
        }

        protected virtual IEnumerable<T> FindAll()
        {
            return _session.Query<T>().ToList();
        }

        protected virtual IEnumerable<T> FindByCriteria(Expression<Func<T, bool>> criteria)
        {
            return QueryByCriteria(criteria).ToList();
        }

        protected virtual IQueryable<T> QueryAll()
        {
            return _session.Query<T>();
        }

        protected virtual IQueryable<T> QueryByCriteria(Expression<Func<T, bool>> criteria)
        {
            return _session.Query<T>().Where(criteria);
        }

        public virtual void Save(T entity)
        {
            _session.SaveOrUpdate(entity);
        }

        public void Remove(T entity)
        {
            _session.Delete(entity);
        }

        public void FlushSession()
        {
            _session.Flush();
        }
    }
}
