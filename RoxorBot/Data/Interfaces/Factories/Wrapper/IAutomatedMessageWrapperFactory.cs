using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Factories.Wrapper
{
    public interface IAutomatedMessageWrapperFactory
    {
        AutomatedMessageWrapper CreateNew(AutomatedMessage model);
        AutomatedMessageWrapper CreateNew(string text, int interval, bool enabled);
    }
}
