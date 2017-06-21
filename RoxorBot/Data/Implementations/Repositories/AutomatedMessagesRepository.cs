using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Database.Repository
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
