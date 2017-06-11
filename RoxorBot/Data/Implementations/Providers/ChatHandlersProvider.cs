using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Providers;

namespace RoxorBot.Data.Implementations.Providers
{
    public class ChatHandlersProvider : IChatHandlersProvider
    {
        private readonly IUnityContainer _container;

        public ChatHandlersProvider(IUnityContainer container)
        {
            _container = container;
        }

        public IEnumerable<IChatCommandHandler> GetAllHandlers()
        {
            var types = GetType().Assembly.GetTypes();
            var handlers = types.Where(x => typeof(IChatCommandHandler).IsAssignableFrom(x));
            var instantiableHandlers = handlers.Where(x => !x.IsAbstract && x.IsClass && x.GetConstructors().Any());
            foreach (var handler in instantiableHandlers)
            {
                yield return (IChatCommandHandler)_container.Resolve(handler);
            }
        }
    }
}
