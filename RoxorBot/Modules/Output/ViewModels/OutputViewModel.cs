using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using Prism.Mvvm;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using RoxorBot.Logic.Logging;

namespace RoxorBot.Modules.Output.ViewModels
{
    public class OutputViewModel : BindableBase
    {
        private readonly ILogger _logger = LoggerProvider.GetLogger();
        private readonly List<string> _logMessages;

        public string Output => string.Join(Environment.NewLine, _logMessages);


        public OutputViewModel(IEventAggregator aggregator)
        {
            _logMessages = new List<string>();

            aggregator.GetEvent<AddLogEvent>().Subscribe(OnAddLog);
        }

        private void OnAddLog(string obj)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}]: {obj}";
            _logger.Info(msg);
            _logMessages.Add(msg);
            RaisePropertyChanged(nameof(Output));
        }
    }
}
