using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IUserCommandsProvider
    {
        UserCommand GetAutomatedMessage(Guid id);
        UserCommand CreateNew(string command, string reply);
    }
}
