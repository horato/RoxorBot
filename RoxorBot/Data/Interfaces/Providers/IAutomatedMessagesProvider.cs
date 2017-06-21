using System;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IAutomatedMessagesProvider
    {
        AutomatedMessage GetAutomatedMessage(Guid id);
        AutomatedMessage CreateNew(string text, int interval, bool enabled);
    }
}
