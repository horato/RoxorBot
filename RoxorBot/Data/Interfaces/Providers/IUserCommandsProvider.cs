using System;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IUserCommandsProvider
    {
        UserCommand GetAutomatedMessage(Guid id);
        UserCommand CreateNew(string command, string reply);
    }
}
