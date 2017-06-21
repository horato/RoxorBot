using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Database;
using RoxorBot.Data.Interfaces.Factories;
using RoxorBot.Data.Interfaces.Providers;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Logic.Managers
{
    public class AutomatedMessagesManager : IAutomatedMessagesManager
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _aggregator;
        private readonly IAutomatedMessageWrapperFactory _wrapperFactory;
        private readonly IAutomatedMessagesRepository _repository;

        private readonly Dictionary<Guid, AutomatedMessageWrapper> _messages;
        public bool IsPaused { get; private set; }
        public bool IsRunning { get; private set; }

        public AutomatedMessagesManager(ILogger logger, IEventAggregator aggregator, IAutomatedMessageWrapperFactory wrapperFactory, IAutomatedMessagesRepository repository)
        {
            _logger = logger;
            _aggregator = aggregator;
            _wrapperFactory = wrapperFactory;
            _repository = repository;

            logger.Log("Initializing MessagesManager...");
            _messages = LoadMessages();
            IsRunning = false;
        }

        public void AddAutomatedMessage(string text, int interval, bool start, bool enabled)
        {
            var message = _wrapperFactory.CreateNew(text, interval, enabled);
            lock (_messages)
                _messages.Add(message.Id, message);
            if (start)
                message.Start();
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

            _logger.Log("Loaded " + result.Count + " automated messages from database.");

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
