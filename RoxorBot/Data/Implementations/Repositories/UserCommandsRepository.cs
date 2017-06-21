using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Database.Repository
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
