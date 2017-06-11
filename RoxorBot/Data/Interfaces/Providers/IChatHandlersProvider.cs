using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Chat;

namespace RoxorBot.Data.Interfaces.Providers
{
    public interface IChatHandlersProvider
    {
        IEnumerable<IChatCommandHandler> GetAllHandlers();
    }
}
