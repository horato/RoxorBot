using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Interfaces.Factories;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Factories
{
    public class AutomatedMessageWrapperFactory : IAutomatedMessageWrapperFactory
    {
        private readonly IEventAggregator _aggregator;
        private readonly IAutomatedMessagesProvider _messagesProvider;

        public AutomatedMessageWrapperFactory(IEventAggregator aggregator, IAutomatedMessagesProvider messagesProvider)
        {
            _aggregator = aggregator;
            _messagesProvider = messagesProvider;
        }

        public AutomatedMessageWrapper CreateNew(AutomatedMessage model)
        {
            return new AutomatedMessageWrapper(model, _aggregator);
        }

        public AutomatedMessageWrapper CreateNew(string text, int interval, bool enabled)
        {
            var model = _messagesProvider.CreateNew(text, interval, enabled);
            return CreateNew(model);
        }
    }
}
