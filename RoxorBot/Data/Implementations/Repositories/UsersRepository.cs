using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using RoxorBot.Data.Implementations.Database.Repository;
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
    }
}
