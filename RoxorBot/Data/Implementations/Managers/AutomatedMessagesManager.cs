using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Implementations.Managers
{
    public class AutomatedMessagesManager : IAutomatedMessagesManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IAutomatedMessageWrapperFactory _wrapperFactory;
        private readonly IAutomatedMessagesRepository _repository;

        private readonly Dictionary<Guid, AutomatedMessageWrapper> _messages;
        public bool IsPaused { get; private set; }
        public bool IsRunning { get; private set; }

        public AutomatedMessagesManager(IEventAggregator aggregator, IAutomatedMessageWrapperFactory wrapperFactory, IAutomatedMessagesRepository repository)
        {
            _aggregator = aggregator;
            _wrapperFactory = wrapperFactory;
            _repository = repository;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing MessagesManager...");
            _messages = LoadMessages();
            IsRunning = false;
        }

        public void AddAutomatedMessage(string text, int interval, bool start, bool enabled)
        {
            lock (_messages)
            {
                var message = _wrapperFactory.CreateNew(text, interval, enabled);
                _messages.Add(message.Id, message);
                if (start)
                    message.Start();
            }
        }

        public void UpdateAutomatedMessage(Guid id, string msg, int interval, bool start, bool enabled)
        {
            if (id == Guid.Empty)
                return;

            UpdateModel(id, msg, interval, enabled);
            if (!_messages.ContainsKey(id))
                return;

            var message = _messages[id];
            message.Stop();
            message.Message = msg;
            message.Interval = interval;
            message.Enabled = enabled;
            if (start)
                message.Start();
        }

        private void UpdateModel(Guid id, string msg, int interval, bool enabled)
        {
            var message = _repository.FindById(id);
            if (message == null)
                return;

            message.Message = msg;
            message.Interval = interval;
            message.Enabled = enabled;
            _repository.Save(message);
            _repository.FlushSession();
        }

        public void RemoveMessage(AutomatedMessageWrapper msg)
        {
            msg.Stop();
            msg.Dispose();

            lock (_messages)
                _messages.Remove(msg.Id);

            _repository.Remove(msg.Model);
            _repository.FlushSession();
        }

        private Dictionary<Guid, AutomatedMessageWrapper> LoadMessages()
        {
            var result = new Dictionary<Guid, AutomatedMessageWrapper>();
            var messages = _repository.GetAll();

            foreach (var msg in messages)
            {
                result.Add(msg.Id, _wrapperFactory.CreateNew(msg));
            }

            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + result.Count + " automated messages from database.");
            return result;
        }

        public void StartAllTimers()
        {
            foreach (var msg in _messages.Values)
                msg.Start();

            IsRunning = true;
            IsPaused = false;
            _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
        }

        public void StopAllTimers()
        {
            foreach (var msg in _messages.Values)
                msg.Stop();

            IsRunning = false;
            IsPaused = false;
            _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
        }

        public void PauseAllTimers()
        {
            StopAllTimers();
            IsPaused = true;
        }

        public List<AutomatedMessageWrapper> GetAllMessages()
        {
            return _messages.Values.ToList();
        }

        public AutomatedMessageWrapper GetMessage(Guid id)
        {
            if (_messages.ContainsKey(id))
                return _messages[id];

            return null;
        }
    }
}
