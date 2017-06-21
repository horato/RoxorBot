using System;
using System.Collections.Generic;
using System.Linq;
using Prism.Events;
using Prism.Mvvm;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Modules.Output.ViewModels
{
    public class OutputViewModel : BindableBase
    {
        private readonly ILogger _logger;
        private readonly List<string> _logMessages;

        public string Output => string.Join(Environment.NewLine, _logMessages);


        public OutputViewModel(IEventAggregator aggregator, ILogger logger)
        {
            _logger = logger;
            _logMessages = new List<string>();

            aggregator.GetEvent<AddLogEvent>().Subscribe(OnAddLog);
        }

        private void OnAddLog(string obj)
        {
            var msg = $"[{DateTime.Now:HH:mm:ss}]: {obj}";
            _logger.Log(msg);
            _logMessages.Add(msg);
            RaisePropertyChanged(nameof(Output));
        }
    }
}
