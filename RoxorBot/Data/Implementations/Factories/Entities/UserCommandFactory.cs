using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Factories.Entities;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Factories.Entities
{
    public class UserCommandFactory : IUserCommandFactory
    {
        public UserCommand CreateNew(string command, string reply)
        {
            return new UserCommand(command, reply);
        }
    }
}
