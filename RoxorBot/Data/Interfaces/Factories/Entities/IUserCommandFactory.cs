using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Factories.Entities
{
    public interface IUserCommandFactory
    {
        UserCommand CreateNew(string command, string reply);
    }
}
