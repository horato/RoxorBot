using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Events;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Logic;
using TwitchLib;

namespace RoxorBot
{
    public class ShellViewModel
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;

        public ICommand ClosingCommand { get; }

        public ShellViewModel(IEventAggregator aggregator, IManagersLoader managersLoader, IUsersManager usersManager)
        {
            TwitchHelper.EnsureTwitchLoginCorrect();
            _aggregator = aggregator;
            _usersManager = usersManager;

            Task.Factory.StartNew(managersLoader.Load);

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
            _usersManager.SaveAll();
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
