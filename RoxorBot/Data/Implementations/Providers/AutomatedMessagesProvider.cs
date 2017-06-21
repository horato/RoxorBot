using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Implementations.Providers
{
    public class AutomatedMessagesProvider : IAutomatedMessagesProvider
    {
        private readonly IAutomatedMessagesRepository _automatedMessagesRepository;

        public AutomatedMessagesProvider(IAutomatedMessagesRepository automatedMessagesRepository)
        {
            _automatedMessagesRepository = automatedMessagesRepository;
        }

        public AutomatedMessage GetAutomatedMessage(Guid id)
        {
            if (id == Guid.Empty)
                return null;

            return _automatedMessagesRepository.FindById(id);
        }

        public AutomatedMessage CreateNew(string text, int interval, bool enabled)
        {
            var msg = new AutomatedMessage(text, interval, enabled);
            _automatedMessagesRepository.Save(msg);
            _automatedMessagesRepository.FlushSession();
            return msg;
        }
    }
}
