using System.Collections.Generic;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Repositories
{
    public interface IAutomatedMessagesRepository : IRepository<AutomatedMessage>
    {
        IEnumerable<AutomatedMessage> GetAll();
    }
}
