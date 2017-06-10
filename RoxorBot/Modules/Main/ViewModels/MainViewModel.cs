using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using IrcDotNet;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Events;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;
using RoxorBot.Logic;
using RoxorBot.Model;
using RoxorBot.Model.JSON;
using Timer = System.Timers.Timer;

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
        public ObservableCollection<string> UsersList { get; } = new ObservableCollection<string>();



        public static event EventHandler<IrcRawMessageEventArgs> ChatMessageReceived;
        private YoutubeWindow plugDjWindow;
        private bool onSettingsPage = false;

        public MainViewModel(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IRewardTimerManager rewardTimerManager, IAutomatedMessagesManager automatedMessagesManager, IFilterManager filterManager, IPointsManager pointsManager, IUserCommandsManager userCommandsManager, IDatabaseManager databaseManager, IFollowersManager followersManager, IYoutubeManager youtubeManager, IUsersManager usersManager, IChatMessageHandler chatMessageHandler)
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
                using (var client = new WebClient { Encoding = Encoding.UTF8 })
                {
                    var json = client.DownloadString("http://tmi.twitch.tv/group/user/roxork0/chatters?rand=" + Environment.TickCount);
                    var chatters = new JavaScriptSerializer().Deserialize<Chatters>(json);
                    _usersManager.InitUsers(chatters.chatters.staff, Role.Saff);
                    _usersManager.InitUsers(chatters.chatters.admins, Role.Admins);
                    _usersManager.InitUsers(chatters.chatters.global_mods, Role.Global_mods);
                    _usersManager.InitUsers(chatters.chatters.moderators, Role.Moderators);
                    _usersManager.InitUsers(chatters.chatters.viewers, Role.Viewers);
                    UpdateOnlineUsersList();
                    AddToConsole("Loaded " + chatters.chatter_count + " online viewers.");
                }
            }
            catch (Exception ee)
            {
                AddToConsole(ee.ToString());
            }

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
            _pointsManager.Save();
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
            UsersList.Clear();
            var users = _usersManager.GetAllUsers();
            var temp = users.Where(x => x.isOnline).Select(x => new User { Name = x.Name, Role = x.Role }).ToList();

            foreach (var u in temp.Where(x => x.Role != Role.Viewers))
                u.Name = "(o) " + u.Name;

            temp = temp.OrderBy(x => x.Name).ToList();
            UsersList.AddRange(temp.Select(x => x.Name));
        }

        public void AddToConsole(string text)
        {
            _aggregator.GetEvent<AddLogEvent>().Publish(text);
        }

        [Command]
        public void Raffle()
        {
            var raffle = new RaffleView();
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
            var settingsPage = new SettingsPageView();
            onSettingsPage = true;
            settingsPage.ShowDialog();
            Properties.Settings.Default.Save();
            onSettingsPage = false;
        }

        [Command]
        public void Commands()
        {
            var control = new CommandsListControl();
            control.ShowDialog();
        }

        [Command]
        public void ShowYoutubeWindow()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.youtubeKey))
                Properties.Settings.Default.youtubeKey = Prompt.ShowDialog("Specify youtube api key", "Api key");
            Properties.Settings.Default.Save();

            if (plugDjWindow == null)
                plugDjWindow = new YoutubeWindow();

            plugDjWindow.Show();
        }
    }
}
