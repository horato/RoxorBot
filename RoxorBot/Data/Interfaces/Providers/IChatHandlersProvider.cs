using System.Collections.Generic;
using RoxorBot.Data.Interfaces.Chat;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IChatHandlersProvider
    {
        IEnumerable<IChatCommandHandler> GetAllHandlers();
    }
}
