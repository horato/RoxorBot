using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories.Entities
{
    public interface IUserCommandFactory
    {
        UserCommand CreateNew(string command, string reply);
    }
}
