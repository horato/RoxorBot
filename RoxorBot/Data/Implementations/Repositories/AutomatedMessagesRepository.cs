using System.Collections.Generic;
using System.Linq;
using NHibernate;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Repositories
{
    public class AutomatedMessagesRepository : Repository<AutomatedMessage>, IAutomatedMessagesRepository
    {
        public AutomatedMessagesRepository(ISessionFactory factory) : base(factory)
        {
        }

        public IEnumerable<AutomatedMessage> GetAll()
        {
            return QueryAll().ToList();
        }
    }
}
