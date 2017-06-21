using System.Collections.Generic;
using System.Linq;
using NHibernate;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Repositories
{
    public class UserCommandsRepository : Repository<UserCommand>, IUserCommandsRepository
    {
        public UserCommandsRepository(ISessionFactory factory) : base(factory)
        {
        }

        public IEnumerable<UserCommand> GetAll()
        {
            return QueryAll().ToList();
        }
    }
}
