using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using IrcDotNet;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using RoxorBot.Model;
using RoxorBot.Model.JSON;

namespace RoxorBot.Data.Implementations
{
    public class ChatMessageHandler : IChatMessageHandler
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IUsersManager _usersManager;

        public ChatMessageHandler(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IUsersManager usersManager)
        {
            _logger = logger;
            _aggregator = aggregator;
            _chatManager = chatManager;
            _usersManager = usersManager;

        }

        public void Init()
        {
            _aggregator.GetEvent<IrcMessageReceived>().Subscribe(OnIrcMessageReceived);
        }

        private void OnIrcMessageReceived(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count < 2)
                return;

            var msg = e.Message.Parameters[1];
            if (e.Message.Command == "PRIVMSG" && e.Message.Parameters[0] == "#roxork0")
            {
                if (msg.Length > Properties.Settings.Default.maxMessageLength && !_usersManager.IsAdmin(e.Message.Source.Name))
                {
                    _chatManager.SendChatMessage(".timeout " + e.Message.Source.Name + " 120", true);
                    if (Properties.Settings.Default.notifyChatRestriction)
                        _chatManager.SendChatMessage("Pls no spamerino " + e.Message.Source.Name + " Keepo");
                    return;
                }
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
                _chatManager.SendChatMessage(e.Message.Source.Name +
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
                            _chatManager.SendChatMessage(
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
                    _chatManager.SendChatMessage(name + " is follower.");
                else
                    _chatManager.SendChatMessage(name + "is not follower.");
            }
            else if (msg.StartsWith("!gettimer ") && _usersManager.IsSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = msg.Split(' ');
                string name = commands[1].ToLower();
                var u = _usersManager.GetUser(name);
                if (u == null)
                    _chatManager.SendChatMessage(e.Message.Source.Name + ": " + name + " not found.");
                else
                    _chatManager.SendChatMessage(e.Message.Source.Name + ": " + u.Name + " has " + u.RewardTimer + " reward timer out of 30.");
            }
        }
    }
}
