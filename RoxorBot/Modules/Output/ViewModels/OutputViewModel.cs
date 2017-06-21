using System;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Modules.Output.ViewModels
{
    public class OutputViewModel
    {
        private readonly ILogger _logger;

        public OutputViewModel(IEventAggregator aggregator, ILogger logger)
        {
            _logger = logger;
            aggregator.GetEvent<AddLogEvent>().Subscribe(OnAddLog);
        }

        private void OnAddLog(string obj)
        {
            var msg = $"[{DateTime.Now:HH: mm:ss}]";
            _logger.Log(msg);
        }
    }
}
