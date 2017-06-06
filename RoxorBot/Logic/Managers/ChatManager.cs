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
        private readonly IRewardTimerManager _rewardTimerManager;
        private readonly IUsersManager _usersManager;
        private readonly List<DateTime> _floodQueue = new List<DateTime>();
        private System.Timers.Timer _floodTimer;
        private System.Timers.Timer _disconnectCheckTimer;
        private int _floodTicksElapsed;

        public bool IsConnecting { get; private set; }
        public bool IsConnected { get; private set; }
        public int FloodQueueCount => _floodQueue.Count;

        public ChatManager(IEventAggregator aggregator, ILogger logger, IRewardTimerManager rewardTimerManager, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _logger = logger;
            _rewardTimerManager = rewardTimerManager;
            _usersManager = usersManager;
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
                if (_disconnectCheckTimer != null)
                    return;

                _disconnectCheckTimer = new System.Timers.Timer(5000);
                _disconnectCheckTimer.AutoReset = false;
                _disconnectCheckTimer.Elapsed += (a, b) =>
                {
                    _aggregator.GetEvent<AddLogEvent>().Publish("Chat blocked, reconnecting...");
                    Disconnect();
                    Connect();
                    if (_rewardTimerManager.IsPaused)
                        _rewardTimerManager.Start();
                };

                _aggregator.GetEvent<ChatConnectionChangedEvent>().Publish(ChatConnectionState.Connected);
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
                _client.RawMessageReceived += c_RawMessageReceived;
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

        void c_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            System.Diagnostics.Debug.Write("RawMessageReceived: Command: " + e.Message.Command + (e.Message.Source == null ? "" : " From: " + e.Message.Source.Name) + " Parameters: ");
            foreach (string s in e.Message.Parameters)
                if (!string.IsNullOrEmpty(s))
                    System.Diagnostics.Debug.Write(s + ",");
            System.Diagnostics.Debug.WriteLine("");

            if (e.Message.Parameters.Count < 2)
                return;

            var msg = e.Message.Parameters[1];
            if (e.Message.Command == "PRIVMSG" && e.Message.Parameters[0] == "#roxork0")
            {
                if (msg.Length > Properties.Settings.Default.maxMessageLength && !_usersManager.IsAdmin(e.Message.Source.Name))
                {
                    SendChatMessage(".timeout " + e.Message.Source.Name + " 120", true);
                    if (Properties.Settings.Default.notifyChatRestriction)
                        SendChatMessage("Pls no spamerino " + e.Message.Source.Name + " Keepo");
                    return;
                }
                _aggregator.GetEvent<IrcMessageReceived>().Publish(e);
                HandleRawMessage(e);
            }
            else if (e.Message.Command == "JOIN")
            {
                _usersManager.AddUser(e.Message.Source.Name, Role.Viewers);
                _usersManager.ChangeOnlineStatus(e.Message.Source.Name, true);
                _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
            }
            else if (e.Message.Command == "PART")
            {
                _usersManager.ChangeOnlineStatus(e.Message.Source.Name, false);
                _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
            }
            else if (e.Message.Command == "MODE")
                HandleMODE(e);
            //else if (e.Message.Command == "366" && e.Message.Parameters[2] == "End of /NAMES list")
            //    sendChatMessage("ItsBoshyTime KAPOW Keepo");
            //else if(e.Message.Command == "PART" && e.Message.Source.Name.ToLower().Contains("roxork0bot"))
            //    c.SendRawMessage("JOIN #roxork0");
            else if (e.Message.Command == "NOTICE")
            //RawMessageReceived: Command: NOTICE From: tmi.twitch.tv Parameters: *,Error logging in,
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
        }

        private void HandleMODE(IrcRawMessageEventArgs e)
        {
            var add = e.Message.Parameters[1].ToLower() == "+o";
            var nick = e.Message.Parameters[2];

            var user = _usersManager.GetUser(nick);
            if (user == null)
                return;

            user.Role = add ? Role.Moderators : Role.Viewers;
            _aggregator.GetEvent<UpdateOnlineUsersList>().Publish();
        }

        private void HandleRawMessage(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count < 2)
                return;

            var msg = e.Message.Parameters[1];
            if (msg == "!since")
            {
                var user = _usersManager.GetUser(e.Message.Source.Name);
                if (user == null || !user.IsFollower || user.IsFollowerSince.Year == 999)
                    return;
                var time = user.IsFollowerSince;
                SendChatMessage(e.Message.Source.Name +
                                string.Format(" is following since {0}.{1:D2}.{2} {3}:{4:D2}:{5:D2}", time.Day,
                                    time.Month, time.Year, time.Hour, time.Minute, time.Second));
            }
            else if (msg == "!uptime")
            {
                try
                {
                    using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                    {
                        string json = client.DownloadString("https://api.twitch.tv/kraken/streams?channel=roxork0");
                        var info = new JavaScriptSerializer().Deserialize<ChannelInfo>(json);
                        if (info.streams.Length < 1)
                            return;

                        var stream = info.streams[0];
                        var start = TimeParser.GetDuration(stream.created_at);
                        if (start.Month != 999)
                        {
                            var time = DateTime.Now - start;
                            SendChatMessage(
                                string.Format(
                                    "Streaming for " + (time.Days > 0 ? "{0}d" : "") + " {1}h {2:D2}m {3:D2}s",
                                    time.Days, time.Hours, time.Minutes, time.Seconds));
                            return;
                        }
                        System.Diagnostics.Debug.WriteLine(
                            "Error !uptime: parsing time: https://api.twitch.tv/kraken/streams?channel=roxork0");
                        _logger.Log("Error !uptime: parsing time: https://api.twitch.tv/kraken/streams?channel=roxork0");
                    }
                }
                catch (Exception ee)
                {
                    System.Diagnostics.Debug.WriteLine(ee.ToString());
                }
            }
            else if (msg.StartsWith("!isfollower ") && _usersManager.IsAdmin(e.Message.Source.Name))
            {
                string[] commands = msg.Split(' ');
                string name = commands[1].ToLower();
                var u = _usersManager.GetUser(name);
                if (u == null || !u.IsFollower)
                    SendChatMessage(name + " is follower.");
                else
                    SendChatMessage(name + "is not follower.");
            }
            else if (msg.StartsWith("!gettimer ") && _usersManager.IsSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = msg.Split(' ');
                string name = commands[1].ToLower();
                var u = _usersManager.GetUser(name);
                if (u == null)
                    SendChatMessage(e.Message.Source.Name + ": " + name + " not found.");
                else
                    SendChatMessage(e.Message.Source.Name + ": " + u.Name + " has " + u.RewardTimer +
                                    " reward timer out of 30.");
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
