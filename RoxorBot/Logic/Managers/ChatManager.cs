using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Interfaces;
using RoxorBot.Model;
using RoxorBot.Model.JSON;
using TwitchLib;
using TwitchLib.Enums;
using TwitchLib.Events.Client;
using TwitchLib.Extensions.Client;
using TwitchLib.Models.Client;

namespace RoxorBot.Logic.Managers
{
    public class ChatManager : IChatManager
    {
        private TwitchClient _client;
        private readonly IEventAggregator _aggregator;
        private readonly ILogger _logger;
        private readonly List<DateTime> _floodQueue = new List<DateTime>();
        private System.Timers.Timer _floodTimer;

        private bool _isConnected;
        private bool _isConnecting;
        private JoinedChannel _channel;

        public bool IsConnecting
        {
            get { return _isConnecting; }
            private set
            {
                _isConnecting = value;
                _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            private set
            {
                _isConnected = value;
                _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
            }
        }

        public int FloodQueueCount => _floodQueue.Count;

        public ChatManager(IEventAggregator aggregator, ILogger logger)
        {
            _aggregator = aggregator;
            _logger = logger;
        }

        public void Connect()
        {
            IsConnecting = true;
            IsConnected = false;
            _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Connecting);

            var connectionTask = Task.Factory.StartNew(DoConnect);
            connectionTask.ContinueWith(task =>
            {
                if (!task.Result)
                {
                    System.Diagnostics.Debug.WriteLine("timed out");
                    IsConnecting = false;
                    IsConnected = false;
                    _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Disconnected);
                    return;
                }

                IsConnecting = false;
                IsConnected = true;


                _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Connected);

            });
        }

        private bool DoConnect()
        {
            //TODO: config
            var channel = "horato2";
            try
            {
                var resetEvent = new ManualResetEventSlim();
                var credentials = new ConnectionCredentials(Properties.Settings.Default.twitch_login, Properties.Settings.Default.twitch_oauth);
                _client = new TwitchClient(credentials, channel, '!', '!', true, false);
                _client.OnConnected += (a, b) =>
                {
                    resetEvent.Set();
                    resetEvent.Dispose();
                };
                _client.OnIncorrectLogin += OnIncorrectLogin;
                _client.OnMessageReceived += OnMessageReceived;
                _client.OnChatCommandReceived += OnChatCommandReceived;
                _client.OnConnected += OnConnected;
                _client.OnSendReceiveData += OnDataReceivedSent;
                _client.OnJoinedChannel += ClientOnOnJoinedChannel;
                _client.OnLeftChannel += ClientOnOnLeftChannel;
                _client.OnUserJoined += ClientOnOnUserJoined;
                _client.OnModeratorJoined += ClientOnOnModeratorJoined;
                _client.OnUserLeft += ClientOnUserLeft;
                _client.OnModeratorLeft += ClientOnOnModeratorLeft;
                _client.OnModeratorsReceived += ClientOnOnModeratorsReceived;
                _client.Connect();

                if (!resetEvent.Wait(10000))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ClientOnOnModeratorsReceived(object sender, OnModeratorsReceivedArgs e)
        {
            if (e?.Moderators == null)
                return;

            _aggregator.GetEvent<ModeratorsListReceivedEvent>().Publish(e.Moderators);
        }

        private void ClientOnOnModeratorLeft(object sender, OnModeratorLeftArgs e)
        {
            if (string.IsNullOrEmpty(e?.Username))
                return;

            _aggregator.GetEvent<ChatUserLeftEvent>().Publish(new ChatUserLeftEventEventArgs(e.Username, true));
        }

        private void ClientOnUserLeft(object sender, OnUserLeftArgs e)
        {
            if (string.IsNullOrEmpty(e?.Username))
                return;

            _aggregator.GetEvent<ChatUserLeftEvent>().Publish(new ChatUserLeftEventEventArgs(e.Username, false));
        }

        private void ClientOnOnModeratorJoined(object sender, OnModeratorJoinedArgs e)
        {
            if (string.IsNullOrEmpty(e?.Username))
                return;

            _aggregator.GetEvent<ChatUserJoinedEvent>().Publish(new ChatUserJoinedEventArgs(e.Username, true));
        }

        private void ClientOnOnUserJoined(object sender, OnUserJoinedArgs e)
        {
            if (string.IsNullOrEmpty(e?.Username))
                return;

            _aggregator.GetEvent<ChatUserJoinedEvent>().Publish(new ChatUserJoinedEventArgs(e.Username, false));
        }

        private void ClientOnOnLeftChannel(object sender, OnLeftChannelArgs e)
        {
            if (string.IsNullOrWhiteSpace(e?.Channel))
                return;
            if (_channel?.Channel == e.Channel)
                _channel = null;
        }

        private void ClientOnOnJoinedChannel(object sender, OnJoinedChannelArgs e)
        {
            if (string.IsNullOrWhiteSpace(e?.Channel))
                return;

            SendChatMessage("ItsBoshyTime KAPOW Keepo");

            _channel = _client.GetJoinedChannel(e.Channel);
            if (_channel == null)
                return;

            _aggregator.GetEvent<ChatChannelJoined>().Publish(_channel);
        }

        private void OnDataReceivedSent(object sender, OnSendReceiveDataArgs e)
        {
            if (e?.Direction != SendReceiveDirection.Sent)
                return;

            System.Diagnostics.Debug.WriteLine("sent " + e.Data);

            lock (_floodQueue)
            {
                _floodQueue.Add(DateTime.Now);
                var temp = _floodQueue.ToList();
                foreach (var item in temp)
                {
                    if (item.AddSeconds(30) > DateTime.Now)
                        continue;

                    _floodQueue.Remove(item);
                }

                _aggregator.GetEvent<UpdateStatusLabelsEvent>().Publish();
            }
        }

        private void OnConnected(object sender, OnConnectedArgs e)
        {
            if (e == null)
                return;

            _aggregator.GetEvent<ChatConnectedEvent>().Publish(e);
        }

        private void OnChatCommandReceived(object sender, OnChatCommandReceivedArgs e)
        {
            if (e?.Command == null)
                return;

            _aggregator.GetEvent<ChatCommandReceivedEvent>().Publish(e.Command);
        }

        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e?.ChatMessage == null)
                return;

            System.Diagnostics.Debug.WriteLine("RECEIVED: " + e.ChatMessage.RawIrcMessage);

            _aggregator.GetEvent<ChatMessageReceivedEvent>().Publish(e.ChatMessage);
        }

        private void OnIncorrectLogin(object sender, OnIncorrectLoginArgs e)
        {
            _aggregator.GetEvent<AddLogEvent>().Publish("Error logging in. Wrong password/oauth?");
        }

        public void SendChatMessage(string message, bool overrideFloodQueue = false)
        {
            if (_floodQueue.Count > 90 && !overrideFloodQueue)
            {
                _logger.Log("Queue limit reached. Ignoring: " + message);
                return;
            }
            _client.SendMessage(message);
        }

        public void Disconnect()
        {
            _floodTimer?.Stop();
            _floodQueue.Clear();

            try
            {
                if (_channel != null)
                    _client.LeaveChannel(_channel);
                _client.Disconnect();
            }
            catch
            {
                //
            }

            IsConnecting = false;
            IsConnected = false;

            _aggregator.GetEvent<UpdateStatusLabelsEvent>().Publish();
            _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Disconnected);
        }

        public void TimeoutUser(string user, TimeSpan duration)
        {
            if (string.IsNullOrWhiteSpace(user))
                return;

            _client.TimeoutUser(user, duration);
        }

        public void BanUser(string user)
        {
            if (string.IsNullOrWhiteSpace(user))
                return;

            _client.BanUser(user);
        }
    }
}
