using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;
using RoxorBot.Logic;
using TwitchLib;

namespace RoxorBot
{
    public class ShellViewModel
    {
        private readonly IEventAggregator _aggregator;
        private readonly IPointsManager _pointsManager;
        private readonly IDatabaseManager _databaseManager;

        public ICommand ClosingCommand { get; }

        public ShellViewModel(IEventAggregator aggregator, IPointsManager pointsManager, IDatabaseManager databaseManager)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.TwitchId))
                Properties.Settings.Default.TwitchId = Prompt.ShowDialog("Specify twitch clientId", "Twitch client Id");
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_oauth))
                Properties.Settings.Default.twitch_oauth = Prompt.ShowDialog("Specify twitch oauth", "Twitch oauth");
            Properties.Settings.Default.Save();

            TwitchAPI.Settings.ClientId = Properties.Settings.Default.TwitchId;
            TwitchAPI.Settings.AccessToken = Properties.Settings.Default.twitch_oauth;

            _aggregator = aggregator;
            _pointsManager = pointsManager;
            _databaseManager = databaseManager;
            ClosingCommand = new DelegateCommand(Closing, CanClosing);

            _aggregator.GetEvent<RaiseButtonsEnabled>().Subscribe(OnRaiseButtonsEnabled);
        }

        private void OnRaiseButtonsEnabled()
        {
            ViewModelProviderHelper.RaiseAllCanExecuteChanged();
            _aggregator.GetEvent<UpdateStatusLabelsEvent>().Publish();
        }

        [Command]
        public void Closing()
        {
            Properties.Settings.Default.Save();
            _pointsManager.Save();
            _databaseManager.Close();
            Mail.sendMail();
            //if (plugDjWindow != null)
            //{
            //    plugDjWindow.close = true;
            //    plugDjWindow.Close();
            //}
            Environment.Exit(0);
        }

        public bool CanClosing()
        {
            return true;
        }
    }
}
