using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Factories.Wrapper
{
    public interface IUserCommandWrapperFactory
    {
        UserCommandWrapper CreateNew(UserCommand model);
        UserCommandWrapper CreateNew(string command, string reply);
    }
}
