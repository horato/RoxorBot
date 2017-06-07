using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IrcDotNet;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using RoxorBot.Model;
using RoxorBot.Model.JSON;

namespace RoxorBot.Logic.Managers
{
    public class ChatManager : IChatManager
    {
        private StandardIrcClient _client;
        private readonly IEventAggregator _aggregator;
        private readonly ILogger _logger;
        private readonly List<DateTime> _floodQueue = new List<DateTime>();
        private System.Timers.Timer _floodTimer;
        private System.Timers.Timer _disconnectCheckTimer;
        private int _floodTicksElapsed;

        public bool IsConnecting { get; private set; }
        public bool IsConnected { get; private set; }
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
                    _client.Dispose();
                    System.Diagnostics.Debug.WriteLine("timed out");
                    IsConnecting = false;
                    IsConnected = false;
                    _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Disconnected);
                    return;
                }

                IsConnecting = false;
                IsConnected = true;

                _floodTimer = new System.Timers.Timer(1000);
                _floodTimer.AutoReset = true;
                _floodTimer.Elapsed += (arg1, arg2) =>
                {
                    _floodTicksElapsed++;
                    if (_disconnectCheckTimer.Enabled || _floodTicksElapsed <= 15)
                        return;

                    _floodTicksElapsed = 0;
                    _disconnectCheckTimer.Start();
                    _client.SendRawMessage("PING tmi.twitch.tv");
                };
                _floodTimer.Start();

                _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Connected);
                if (_disconnectCheckTimer != null)
                    return;

                _disconnectCheckTimer = new System.Timers.Timer(5000);
                _disconnectCheckTimer.AutoReset = false;
                _disconnectCheckTimer.Elapsed += (a, b) =>
                {
                    _aggregator.GetEvent<AddLogEvent>().Publish("Chat blocked, reconnecting...");
                    Disconnect();
                    Connect();
                };
            });
        }

        private bool DoConnect()
        {
            try
            {
                var resetEvent = new ManualResetEventSlim();

                _client = new StandardIrcClient();
                _client.Connected += (a, b) =>
                {
                    resetEvent.Set();
                    resetEvent.Dispose();
                };
                _client.RawMessageReceived += OnRawMessageReceived;
                _client.RawMessageSent += Client_RawMessageSent;
                _client.Connect("irc.twitch.tv", 6667, false, new IrcUserRegistrationInfo
                {
                    UserName = Properties.Settings.Default.twitch_login,
                    NickName = Properties.Settings.Default.twitch_login,
                    Password = Properties.Settings.Default.twitch_oauth
                });

                if (resetEvent.Wait(10000))
                    return false;

                _client.SendRawMessage("CAP REQ :twitch.tv/membership");
                _client.SendRawMessage("CAP REQ :twitch.tv/commands");

                _client.SendRawMessage("JOIN #roxork0");
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void OnRawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            System.Diagnostics.Debug.Write("RawMessageReceived: Command: " + e.Message.Command + (e.Message.Source == null ? "" : " From: " + e.Message.Source.Name) + " Parameters: ");
            foreach (string s in e.Message.Parameters)
                if (!string.IsNullOrEmpty(s))
                    System.Diagnostics.Debug.Write(s + ",");
            System.Diagnostics.Debug.WriteLine("");

            var msg = e.Message.Parameters[1];
            //RawMessageReceived: Command: NOTICE From: tmi.twitch.tv Parameters: *,Error logging in,
            if (e.Message.Command == "NOTICE")
            {
                if (msg.ToLower().Contains("error logging in"))
                {
                    _aggregator.GetEvent<AddLogEvent>().Publish("Error logging in. Wrong password/oauth?");
                    Disconnect();
                }
                else
                {
                    string notice = "";
                    foreach (string s in e.Message.Parameters)
                        if (!string.IsNullOrEmpty(s))
                            notice += s + ",";
                    _logger.Log("NOTICE RECEIVED: " + notice);
                }
            }
            else if (e.Message.Command == "PING")
            {
                _client.SendRawMessage("PONG tmi.twitch.tv");
            }
            else if (e.Message.Command == "PONG")
            {
                _disconnectCheckTimer.Stop();
            }
            else
            {
                _aggregator.GetEvent<IrcMessageReceived>().Publish(e);
            }
        }

        public void SendChatMessage(string message, bool overrideFloodQueue = false)
        {
            if (_floodQueue.Count > 90 && !overrideFloodQueue)
            {
                _logger.Log("Queue limit reached. Ignoring: " + message);
                return;
            }
            _client.SendRawMessage("PRIVMSG #roxork0 :" + message);
        }

        private void Client_RawMessageSent(object sender, IrcRawMessageEventArgs e)
        {
            if (e != null)
                System.Diagnostics.Debug.WriteLine("sent " + e.RawContent);

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

        public void Disconnect()
        {
            _floodTimer?.Stop();
            _disconnectCheckTimer.Stop();
            _floodQueue.Clear();

            try
            {
                _client.SendRawMessage("PART #roxork0");
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
    }
}
