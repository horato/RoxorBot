using System.Collections.Generic;
using System.Linq;
using NHibernate;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Repositories
{
    public class UsersRepository : Repository<User>, IUsersRepository
    {
        public UsersRepository(ISessionFactory factory) : base(factory)
        {
        }

        public IEnumerable<User> GetAll()
        {
            return QueryAll().ToList();
        }

        public void Create(IEnumerable<User> users)
        {
            using (var session = SessionFactory.OpenStatelessSession())
            using (var tran = session.BeginTransaction())
            {
                try
                {
                    foreach (var user in users)
                        session.Insert(user);

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    session.Close();
                }
            }
        }

        public void SaveAll(IEnumerable<User> users)
        {
            var list = users?.ToList();
            if (list == null)
                return;

            using (var session = SessionFactory.OpenStatelessSession())
            using (var tran = session.BeginTransaction())
            {
                try
                {
                    foreach (var user in list)
                    {
                        if (user.IsTransient())
                            session.Insert(user);
                        else
                            session.Update(user);
                    }

                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                finally
                {
                    session.Close();
                }
            }
        }
    }
}
