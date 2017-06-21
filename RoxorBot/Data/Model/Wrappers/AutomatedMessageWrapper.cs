using System;
using System.Timers;
using Prism.Events;
using RoxorBot.Data.Base;
using RoxorBot.Data.Events;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Model.Wrappers
{
    public class AutomatedMessageWrapper : WrapperBase<AutomatedMessage>, IDisposable
    {
        private readonly IEventAggregator _aggregator;
        private readonly Timer _timer;

        public Guid Id => Model.Id;
        public string Message { get { return GetValue<string>(); } set { SetValue(value); } }
        public int Interval { get { return GetValue<int>(); } set { SetValue(value); } }
        public bool Enabled { get { return GetValue<bool>(); } set { SetValue(value); } }
        public bool IsRunning => _timer.Enabled;

        public AutomatedMessageWrapper(AutomatedMessage msg, IEventAggregator aggregator) : base(msg)
        {
            _aggregator = aggregator;
            _timer = new Timer(Interval * 60 * 1000);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerElapsed;
        }

        ~AutomatedMessageWrapper()
        {
            Dispose();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Enabled)
                return;

            _aggregator.GetEvent<AnnounceAutomatedMessage>().Publish(this);
        }

        public void Dispose()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public void Start()
        {
            if (!_timer.Enabled)
                _timer.Start();
        }

        public void Stop()
        {
            if (_timer.Enabled)
                _timer.Stop();
        }
    }
}
