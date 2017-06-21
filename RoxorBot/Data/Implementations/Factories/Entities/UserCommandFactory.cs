using RoxorBot.Data.Interfaces.Factories.Entities;
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
