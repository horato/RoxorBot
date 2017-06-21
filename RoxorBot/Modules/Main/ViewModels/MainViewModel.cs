using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Prism.Events;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Model.Wrappers;
using RoxorBot.Logic;
using TwitchLib.Models.Client;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _aggregator;
        private readonly IRaffleManager _raffleManager;
        private readonly IChatManager _chatManager;
        private readonly IRewardTimerManager _rewardTimerManager;
        private readonly IAutomatedMessagesManager _automatedMessagesManager;
        private readonly IPointsManager _pointsManager;
        private readonly IUsersManager _usersManager;

        private string _statusText = "Not connected";
        private string _floodQueueCount = "Messages sent in last 30s: 0";

        public string StatusText
        {
            get { return _statusText; }
            private set
            {
                _statusText = value;
                RaisePropertyChanged();
            }
        }

        public string FloodQueueCount
        {
            get { return _floodQueueCount; }
            set
            {
                _floodQueueCount = value;
                RaisePropertyChanged();
            }
        }

        public bool IsRewardTimerRunning => _rewardTimerManager.IsRunning;
        public bool IsRewardTimerPaused => _rewardTimerManager.IsPaused;
        public bool AreAutomatedMessagesRunning => _automatedMessagesManager.IsRunning;
        public bool AreAutomatedMessagesPaused => _automatedMessagesManager.IsPaused;
        public ObservableCollection<UserDisplayWrapper> UsersList { get; } = new ObservableCollection<UserDisplayWrapper>();


        private Views.YoutubeView _youtubeWindow;

        public MainViewModel(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IRewardTimerManager rewardTimerManager, IAutomatedMessagesManager automatedMessagesManager, IFilterManager filterManager, IPointsManager pointsManager, IUserCommandsManager userCommandsManager, IFollowersManager followersManager, IYoutubeManager youtubeManager, IUsersManager usersManager, IChatMessageHandler chatMessageHandler)
        {
            _logger = logger;
            _aggregator = aggregator;
            _chatManager = chatManager;
            _rewardTimerManager = rewardTimerManager;
            _automatedMessagesManager = automatedMessagesManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;
            
            chatMessageHandler.Init();

            _aggregator.GetEvent<UpdateStatusLabelsEvent>().Subscribe(UpdateStatusLabels);
            _aggregator.GetEvent<UpdateOnlineUsersList>().Subscribe(UpdateOnlineUsersList);
            _aggregator.GetEvent<ChatChannelJoined>().Subscribe(OnChatConnected);
            // var x = Regex.Match("http://www.twitch.tv/", @"((http:|https:[/][/]|www.)([a-z]|[A-Z]|[0-9]|[/.]|[~])*)");

            _logger.Log("Program init...");

            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.youtubeKey))
                Properties.Settings.Default.youtubeKey = Prompt.ShowDialog("Specify youtube api key", "Api key");
            Properties.Settings.Default.Save();

            new Thread(Load).Start();
        }

        private void Load()
        {
            _logger.Log("Program init finished. Keep in mind that followers and backup playlist are still loading!");
        }

        [Command]
        public void Connect()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_login))
                Properties.Settings.Default.twitch_login = Prompt.ShowDialog("Specify twitch login name", "Login");
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_oauth))
                Properties.Settings.Default.twitch_oauth = Prompt.ShowDialog("Specify twitch oauth", "Oauth");
            Properties.Settings.Default.Save();

            StatusText = "Connecting...";

            try
            {
                _chatManager.Connect();
                Whispers.connect();
                StatusText = "Connected";

                if (_rewardTimerManager.IsPaused)
                    _rewardTimerManager.Start();

                if (_automatedMessagesManager.IsPaused)
                    _automatedMessagesManager.StartAllTimers();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                Disconnect();
            }
        }

        public bool CanConnect()
        {
            return !_chatManager.IsConnecting && !_chatManager.IsConnected;
        }

        [Command]
        public void TimerRewardStart()
        {
            _rewardTimerManager.Start();
            UpdateStatusLabels();
        }

        public bool CanTimerRewardStart()
        {
            return _chatManager.IsConnected && !_rewardTimerManager.IsRunning;
        }

        [Command]
        public void TimerRewardStop(object sender, RoutedEventArgs e)
        {
            _rewardTimerManager.Stop();
            UpdateStatusLabels();
        }

        public bool CanTimerRewardStop()
        {
            return _chatManager.IsConnected && _rewardTimerManager.IsRunning;
        }

        [Command]
        public void Disconnect()
        {
            _rewardTimerManager.Pause();
            _automatedMessagesManager.PauseAllTimers();
            _usersManager.SaveAll();
            _chatManager.Disconnect();
            Whispers.disconnect();
        }

        public bool CanDisconnect()
        {
            return _chatManager.IsConnected;
        }

        private void UpdateStatusLabels()
        {
            FloodQueueCount = "Messages sent in last 30s: " + _chatManager.FloodQueueCount;
            RaisePropertyChanged(nameof(IsRewardTimerPaused));
            RaisePropertyChanged(nameof(IsRewardTimerRunning));
            RaisePropertyChanged(nameof(AreAutomatedMessagesPaused));
            RaisePropertyChanged(nameof(AreAutomatedMessagesRunning));
        }

        private void UpdateOnlineUsersList()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                UsersList.Clear();
                var users = _usersManager.GetAllUsers();
                var temp = users.Where(x => x.IsOnline).Select(x => new UserDisplayWrapper(x)).ToList();
                foreach (var u in temp.Where(x => x.Role != Role.Viewers))
                    u.DisplayAsModerator();

                temp = temp.OrderBy(x => x.Name).ToList();
                UsersList.AddRange(temp);
            }));
        }

        private void OnChatConnected(JoinedChannel e)
        {
            _usersManager.InitUsers(e);
            UpdateOnlineUsersList();
            AddToConsole("Loaded " + UsersList.Count + " online viewers.");
        }

        public void AddToConsole(string text)
        {
            _aggregator.GetEvent<AddLogEvent>().Publish(text);
        }

        [Command]
        public void Raffle()
        {
            var raffle = new Views.RaffleView();
            raffle.ShowDialog();
        }

        public bool CanRaffle()
        {
            return _chatManager.IsConnected;
        }

        [Command]
        public void AutomatedMessagesStart()
        {
            _automatedMessagesManager.StartAllTimers();
            UpdateStatusLabels();
        }

        public bool CanAutomatedMessagesStart()
        {
            return _chatManager.IsConnected && !_automatedMessagesManager.IsRunning;
        }

        [Command]
        public void AutomatedMessagesStop()
        {
            _automatedMessagesManager.StopAllTimers();
            UpdateStatusLabels();
        }

        public bool CanAutomatedMessagesStop()
        {
            return _chatManager.IsConnected && _automatedMessagesManager.IsRunning;
        }

        [Command]
        public void Settings()
        {
            var settingsPage = new Views.SettingsPageView();
            settingsPage.ShowDialog();
            Properties.Settings.Default.Save();
        }

        [Command]
        public void Commands()
        {
            var control = new Controls.CommandsListControl();
            control.ShowDialog();
        }

        [Command]
        public void ShowYoutubeWindow()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.youtubeKey))
                Properties.Settings.Default.youtubeKey = Prompt.ShowDialog("Specify youtube api key", "Api key");
            Properties.Settings.Default.Save();

            if (_youtubeWindow == null)
                _youtubeWindow = new Views.YoutubeView();

            _youtubeWindow.Show();
        }
    }
}
