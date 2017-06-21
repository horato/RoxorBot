using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Database
{
    public interface IAutomatedMessagesRepository : IRepository<AutomatedMessage>
    {
        IEnumerable<AutomatedMessage> GetAll();
    }
}
