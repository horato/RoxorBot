using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IAutomatedMessagesProvider
    {
        AutomatedMessage GetAutomatedMessage(Guid id);
        AutomatedMessage CreateNew(string text, int interval, bool enabled);
    }
}
