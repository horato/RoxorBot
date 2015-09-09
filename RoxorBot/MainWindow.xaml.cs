/*
   Twitch chat bot for #RoXoRk0 
   
   App template by:﻿﻿﻿
   Houssem Dellai    
   houssem.dellai@ieee.org    
   +216 95 325 964    
   Studying Software Engineering    
   in the National Engineering School of Sfax (ENIS) 
   -------------------------------------------------- 
   Curently designed to run in debug mode.
   
   author: horato
   email: horato@seznam.cz
   github: http://github.com/horato
*/


using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Net;
using System.Windows.Threading;
using System.Threading;
using IrcDotNet;
using System.Windows.Input;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Timers;
using System.Web.Script.Serialization;
using HtmlAgilityPack;
using MahApps.Metro.Controls;
using RoxorBot.Model;
using System.Text.RegularExpressions;
using RoxorBot.Model.JSON;
using System.Reflection;
using System.Net.Sockets;
using System.Windows.Media;

namespace RoxorBot
{
    public partial class MainWindow : MetroWindow
    {
        private StandardIrcClient c;

        private List<DateTime> queue;
        private System.Timers.Timer floodTimer;
        private System.Timers.Timer rewardTimer;
        private System.Timers.Timer disconnectCheckTimer;

        private int floodTicksElapsed = 0;
        private int timerReward; //per 30m

        private delegate void ListChanged();
        private event ListChanged OnListChanged;
        public static event EventHandler<IrcRawMessageEventArgs> ChatMessageReceived;

        public MainWindow()
        {
            WriteToLog.ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WriteToLog.LogfileName = "RoxorBot.Log";
            WriteToLog.CreateLogFile();

            AppDomain.CurrentDomain.FirstChanceException += Logger.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += Logger.CurrentDomain_UnhandledException;
            //var x = Regex.Match("http://www.twitch.tv/", @"^(https?://)?([\da-z.-]+).([a-z.]{2,6})([/\w .-]*)*/?$");
            addToConsole("Initializing...");

            InitializeComponent();
            queue = new List<DateTime>();
            OnListChanged += MainWindow_OnListChanged;
            timerReward = Properties.Settings.Default.timerReward;

            new Thread(new ThreadStart(load)).Start();
        }

        private void load()
        {
            DatabaseManager.getInstance();
            UsersManager.getInstance();
            PointsManager.getInstance();
            addToConsole("Loaded " + PointsManager.getInstance().getUsersCount() + " viewers from database.");
            FilterManager.getInstance();
            addToConsole("Loaded " + FilterManager.getInstance().getFiltersCount() + " filtered words from database.");
            FollowerManager.getInstance();
            addToConsole("Loaded " + FollowerManager.getInstance().getFollowersCount() + " followers.");
            MessagesManager.getInstance().setReference(this);
            addToConsole("Loaded " + MessagesManager.getInstance().getMessagesCount() + " messages from database.");
            RaffleManager.getInstance().setReference(this);
            addToConsole("Init finished.");

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                Connect_Button.IsEnabled = true;
                SettingsButton.IsEnabled = true;
            }));
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_login))
                Properties.Settings.Default.twitch_login = Prompt.ShowDialog("Specify twitch login name", "Login");
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_oauth))
                Properties.Settings.Default.twitch_oauth = Prompt.ShowDialog("Specify twitch oauth", "Oauth");
            Properties.Settings.Default.Save();

            tbStatus.Text = "Connecting...";
            Connect_Button.IsEnabled = false;

            try
            {
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString("http://tmi.twitch.tv/group/user/roxork0/chatters?rand=" + Environment.TickCount);
                    Chatters chatters = new JavaScriptSerializer().Deserialize<Chatters>(json);
                    UsersManager.getInstance().initUsers(chatters.chatters.staff, Role.Saff);
                    UsersManager.getInstance().initUsers(chatters.chatters.admins, Role.Admins);
                    UsersManager.getInstance().initUsers(chatters.chatters.global_mods, Role.Global_mods);
                    UsersManager.getInstance().initUsers(chatters.chatters.moderators, Role.Moderators);
                    UsersManager.getInstance().initUsers(chatters.chatters.viewers, Role.Viewers);

                    if (OnListChanged != null)
                        OnListChanged();

                    addToConsole("Loaded " + chatters.chatter_count + " online viewers.");
                }
            }
            catch (Exception ee)
            {
                addToConsole(ee.ToString());
            }

            new Thread(() =>
            {
                try
                {
                    c = new StandardIrcClient();
                    var connectedEvent = new ManualResetEventSlim(false);
                    IPEndPoint point = new IPEndPoint(Dns.GetHostAddresses("irc.twitch.tv")[0], 6667);

                    c.Connected += (sender2, e2) => connectedEvent.Set();
                    c.RawMessageReceived += c_RawMessageReceived;
                    c.RawMessageSent += (arg1, arg2) =>
                    {
                        if (arg2 != null)
                            System.Diagnostics.Debug.WriteLine("sent " + arg2.RawContent);
                        lock (queue)
                            queue.Add(DateTime.Now);
                    };
                    c.Connect(point, false, new IrcUserRegistrationInfo()
                        {
                            UserName = Properties.Settings.Default.twitch_login,
                            NickName = Properties.Settings.Default.twitch_login,
                            Password = Properties.Settings.Default.twitch_oauth
                        });
                    if (!connectedEvent.Wait(10000))
                    {
                        c.Dispose();
                        System.Diagnostics.Debug.WriteLine("timed out");
                        Connect_Button.IsEnabled = true;
                        return;
                    }

                    floodTimer = new System.Timers.Timer(1000);
                    floodTimer.AutoReset = true;
                    floodTimer.Elapsed += (arg1, arg2) =>
                    {
                        floodTicksElapsed++;
                        if (!disconnectCheckTimer.Enabled && floodTicksElapsed > 15)
                        {
                            floodTicksElapsed = 0;
                            disconnectCheckTimer.Start();
                            c.SendRawMessage("PING tmi.twitch.tv");
                        }

                        lock (queue)
                        {
                            List<DateTime> temp = new List<DateTime>();
                            foreach (var item in queue)
                                temp.Add(item);
                            foreach (var item in temp)
                                if (item.AddSeconds(30) < DateTime.Now)
                                    queue.Remove(item);
                        }
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                        {
                            updateStatusLabels();
                        }));

                    };
                    floodTimer.Start();

                    c.SendRawMessage("CAP REQ :twitch.tv/membership");
                    c.SendRawMessage("CAP REQ :twitch.tv/commands");

                    c.SendRawMessage("JOIN #roxork0");

                    Whispers.connect();
                    MessagesManager.getInstance().startAllTimers();

                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        tbStatus.Text = "Connected";

                        Disconnect_Button.IsEnabled = true;
                        if (!Stop_Button.IsEnabled)
                            Start_Button.IsEnabled = true;
                        RaffleButton.IsEnabled = true;
                        AutomatedMessagesButton_Stop.IsEnabled = true;
                        AutomatedMessagesButton_Start.IsEnabled = false;
                    }));

                    if (!(disconnectCheckTimer == null))
                        return;
                    disconnectCheckTimer = new System.Timers.Timer(5000);
                    disconnectCheckTimer.AutoReset = false;
                    disconnectCheckTimer.Elapsed += (a, b) =>
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                addToConsole("Chat blocked, reconnecting...");
                                Disconnect_Click(null, null);
                                Connect_Click(null, null);
                                if (Stop_Button.IsEnabled)
                                    rewardTimer.Start();
                            }));
                    };
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                    System.Diagnostics.Debug.WriteLine(exc.ToString());
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                    {
                        Connect_Button.IsEnabled = true;
                    }));
                }
            }).Start();
        }

        private void Start_Button_OnClick(object sender, RoutedEventArgs e)
        {
            rewardTimer = new System.Timers.Timer(5 * 60 * 1000);
            rewardTimer.AutoReset = true;
            rewardTimer.Elapsed += (a, b) =>
            {
                var users = UsersManager.getInstance().getAllUsers();

                foreach (User user in users)
                {
                    if (user.IsFollower && user.isOnline)
                    {
                        if (user.RewardTimer < 25)
                        {
                            user.RewardTimer += 5;
                        }
                        else
                        {
                            user.RewardTimer = 0;
                            PointsManager.getInstance().addPoints(user.InternalName, timerReward);
                            Whispers.sendPrivateMessage(user.InternalName, "You were awarded " + timerReward + " points for staying with us another 30 minutes.");
                        }
                    }
                }

                addToConsole("Timer tick.");
            };
            rewardTimer.Start();

            Start_Button.IsEnabled = false;
            Stop_Button.IsEnabled = true;
            updateStatusLabels();
        }

        private void Stop_Button_OnClick(object sender, RoutedEventArgs e)
        {
            rewardTimer.Stop();
            Start_Button.IsEnabled = true;
            Stop_Button.IsEnabled = false;
            updateStatusLabels();
        }

        void c_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            System.Diagnostics.Debug.Write("RawMessageReceived: Command: " + e.Message.Command + (e.Message.Source == null ? "" : " From: " + e.Message.Source.Name) + " Parameters: ");
            foreach (string s in e.Message.Parameters)
                if (!string.IsNullOrEmpty(s))
                    System.Diagnostics.Debug.Write(s + ",");
            System.Diagnostics.Debug.WriteLine("");

            if (e.Message.Command == "PRIVMSG" && e.Message.Parameters[0] == "#roxork0")
            {
                if (ChatMessageReceived != null)
                    ChatMessageReceived(this, e);
                handleRawMessage(e);
            }
            else if (e.Message.Command == "JOIN")
            {
                UsersManager.getInstance().addUser(e.Message.Source.Name, Role.Viewers);
                UsersManager.getInstance().changeOnlineStatus(e.Message.Source.Name, true);

                if (OnListChanged != null)
                    OnListChanged();
            }
            else if (e.Message.Command == "PART")
            {
                UsersManager.getInstance().changeOnlineStatus(e.Message.Source.Name, false);

                if (OnListChanged != null)
                    OnListChanged();
            }
            else if (e.Message.Command == "MODE")
                handleMODE(e);
            //else if (e.Message.Command == "366" && e.Message.Parameters[2] == "End of /NAMES list")
            //    sendChatMessage("ItsBoshyTime KAPOW Keepo");
            //else if(e.Message.Command == "PART" && e.Message.Source.Name.ToLower().Contains("roxork0bot"))
            //    c.SendRawMessage("JOIN #roxork0");
            else if (e.Message.Command == "NOTICE") //RawMessageReceived: Command: NOTICE From: tmi.twitch.tv Parameters: *,Error logging in,
            {
                if (e.Message.Parameters[1].ToLower().Contains("error logging in"))
                {
                    addToConsole("Error logging in. Wrong password/oauth?");
                    Disconnect_Click(null, null);
                }
                else
                {
                    string msg = "";
                    foreach (string s in e.Message.Parameters)
                        if (!string.IsNullOrEmpty(s))
                            msg += s + ",";
                    addToConsole("NOTICE RECEIVED: " + msg);
                    Logger.Log("NOTICE RECEIVED: " + msg);
                }
            }
            else if (e.Message.Command == "PING")
            {
                c.SendRawMessage("PONG tmi.twitch.tv");
            }
            else if (e.Message.Command == "PONG")
            {
                disconnectCheckTimer.Stop();
            }
        }

        private void handleMODE(IrcRawMessageEventArgs e)
        {
            bool add = e.Message.Parameters[1].ToLower() == "+o";
            string nick = e.Message.Parameters[2];

            User user = UsersManager.getInstance().getUser(nick);
            if (user == null)
                return;

            user.Role = add ? Role.Moderators : Role.Viewers;

            if (OnListChanged != null)
                OnListChanged();
        }

        private void handleRawMessage(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters[1] == "!since")
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string json = client.DownloadString("https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
                        Followers followers = new JavaScriptSerializer().Deserialize<Followers>(json);
                        if (followers.status != 0)
                        {
                            System.Diagnostics.Debug.WriteLine("Error !since: error: " + followers.error + " Message: " + followers.message + ":::   " + "https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
                            Logger.Log("Error !since: error: " + followers.error + " Message: " + followers.message + ":::   " + "https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
                        }
                        else
                        {
                            DateTime time = TimeParser.GetDuration(followers.created_at);
                            if (time.Year == 999)
                            {
                                System.Diagnostics.Debug.WriteLine("Error !since: parsing time: " + "https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
                                Logger.Log("Error !since: parsing time: " + "https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
                            }
                            else
                                sendChatMessage(e.Message.Source.Name + string.Format(" is following since {0}.{1:D2}.{2} {3}:{4:D2}:{5:D2}", time.Day, time.Month, time.Year, time.Hour, time.Minute, time.Second));
                        }

                    }
                }
                catch (Exception ee)
                {
                    System.Diagnostics.Debug.WriteLine(ee.ToString());
                }
            }
            else if (e.Message.Parameters[1] == "!uptime")
            {
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        string json = client.DownloadString("https://api.twitch.tv/kraken/streams?channel=roxork0");
                        var info = new JavaScriptSerializer().Deserialize<ChannelInfo>(json);
                        if (info.streams.Length < 1)
                            return;

                        var stream = info.streams[0];
                        var start = TimeParser.GetDuration(stream.created_at, false);
                        if (start.Month != 999)
                        {
                            var time = DateTime.Now - start;
                            sendChatMessage(string.Format("Streaming for " + (time.Days > 0 ? "{0}d" : "") + " {1}h {2:D2}m {3:D2}s", time.Days, time.Hours, time.Minutes, time.Seconds));
                            return;
                        }
                        System.Diagnostics.Debug.WriteLine("Error !uptime: parsing time: https://api.twitch.tv/kraken/streams?channel=roxork0");
                        Logger.Log("Error !uptime: parsing time: https://api.twitch.tv/kraken/streams?channel=roxork0");
                    }
                }
                catch (Exception ee)
                {
                    System.Diagnostics.Debug.WriteLine(ee.ToString());
                }
            }
            else if (e.Message.Parameters[1].StartsWith("!isfollower "))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                var u = UsersManager.getInstance().getUser(name);
                if (u == null || !u.IsFollower)
                    sendChatMessage("いいえ, " + name + "さんはともでわありません。");
                else
                    sendChatMessage("はい, " + name + "さんはともです。");
            }
            else if (e.Message.Parameters[1].StartsWith("!gettimer "))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                var u = UsersManager.getInstance().getUser(name);
                if (u == null)
                    sendChatMessage(e.Message.Source.Name + ": " + name + " not found.");
                else
                    sendChatMessage(e.Message.Source.Name + ": " + u.Name + " has " + u.RewardTimer + " reward timer out of 30.");
            }
        }

        public void sendChatMessage(string message)
        {
            if (queue.Count > 90)
            {
                addToConsole("Queue limit reached. Ignoring: " + message);
                Logger.Log("Queue limit reached. Ignoring: " + message);
                return;
            }
            c.SendRawMessage("PRIVMSG #roxork0 :" + message);
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            PointsManager.getInstance().save();
            MessagesManager.getInstance().stopAllTimers();
            if (floodTimer != null)
                floodTimer.Stop();

            if (!Stop_Button.IsEnabled)
                Start_Button.IsEnabled = false;
            Disconnect_Button.IsEnabled = false;
            RaffleButton.IsEnabled = false;
            Connect_Button.IsEnabled = true;

            AutomatedMessagesButton_Stop.IsEnabled = false;
            AutomatedMessagesButton_Start.IsEnabled = false;
            updateStatusLabels();

            FloodQueueCount.Content = 0;

            try
            {
                c.SendRawMessage("PART #roxork0");
                c.Disconnect();
            }
            catch (Exception) { }
            Whispers.disconnect();
        }

        private void updateStatusLabels()
        {
            FloodQueueCount.Content = "Messages sent in last 30s: " + queue.Count;
            if (rewardTimer != null && rewardTimer.Enabled)
            {
                TimerLabel.Foreground = new SolidColorBrush(Colors.Green);
                TimerLabel.Content = "Timer is running.";
            }
            else
            {
                TimerLabel.Foreground = new SolidColorBrush(Colors.Red);
                TimerLabel.Content = "Timer is not running.";
            }
            if (MessagesManager.getInstance().isActive())
            {
                AutomatedMessagesLabel.Foreground = new SolidColorBrush(Colors.Green);
                AutomatedMessagesLabel.Content = "Automated messages are being sent.";
            }
            else
            {
                AutomatedMessagesLabel.Foreground = new SolidColorBrush(Colors.Red);
                AutomatedMessagesLabel.Content = "Automated messages are stopped.";
            }
        }

        private void MainWindow_OnListChanged()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                UsersListView.Items.Clear();
                var temp = new List<User>();
                var users = UsersManager.getInstance().getAllUsers();

                foreach (User u in users)
                    if (u.isOnline)
                        temp.Add(new User { Name = u.Name, InternalName = u.InternalName, Role = u.Role, isOnline = u.isOnline, Points = u.Points, IsFollower = u.IsFollower });

                foreach (User u in temp)
                    if (u.Role != Role.Viewers)
                        u.Name = "(o) " + u.Name;

                temp = temp.OrderBy(x => x.Name).ToList();

                foreach (User user in temp)
                    UsersListView.Items.Add(user.Name);
            }));
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            drawFilters();
            drawMessages();
            drawPoints();
            SettingsContentControl.Content = new SettingsControl().Content;

            MainGrid.Opacity = 0.1;
            MainGrid.IsEnabled = false;
            SettingsGrid.Visibility = Visibility.Visible;
        }

        private void CloseSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            MainGrid.Opacity = 1;
            MainGrid.IsEnabled = true;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void AddFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddFilterDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.FilterWordBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.DurationBox.Text))
                    return;
                OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;

                FilterManager.getInstance().addFilterWord(dialog.FilterWordBox.Text, int.Parse(dialog.DurationBox.Text), "AdminConsole", (bool)dialog.IsRegexCheckBox.IsChecked);

                drawFilters();
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            OverlayContainer.Content = dialog;
            OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AddAutomatedMessage_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddMessageDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.MessageBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.IntervalBox.Text))
                    return;
                OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;

                MessagesManager.getInstance().addFilterWord(dialog.MessageBox.Text, int.Parse(dialog.IntervalBox.Text), (c != null && c.IsConnected));

                drawMessages();
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            OverlayContainer.Content = dialog;
            OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AutomatedMessageDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var msg = dg.SelectedItem as AutomatedMessage;
            if (msg == null)
                return;

            if (Prompt.Ask("Do you wish to delete selected message?", "Delete"))
            {
                MessagesManager.getInstance().removeMessage(msg);
                drawMessages();
            }
        }
        private void drawMessages()
        {
            AutomatedMessagesDataGrid.Items.Clear();
            var msgs = MessagesManager.getInstance().getAllMessages();
            foreach (AutomatedMessage item in msgs)
                AutomatedMessagesDataGrid.Items.Add(item);
        }

        private void drawFilters()
        {
            FilterListDataGrid.Items.Clear();
            var filters = FilterManager.getInstance().getAllFilters(FilterMode.All);
            foreach (FilterItem item in filters)
                FilterListDataGrid.Items.Add(item);
        }

        private void drawPoints()
        {
            PointsDataGrid.Items.Clear();
            var msgs = UsersManager.getInstance().getAllUsers();
            foreach (User item in msgs)
                if (item.Points > 0)
                    PointsDataGrid.Items.Add(item);
        }

        private void FilterListDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var filterItem = dg.SelectedItem as FilterItem;
            if (filterItem == null)
                return;

            if (Prompt.Ask("Do you wish to delete " + filterItem.word + "?", "Delete"))
            {
                FilterManager.getInstance().removeFilterWord(filterItem.word);
                drawFilters();
            }
        }

        private void PointsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            return;
            //Todo
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var filterItem = dg.SelectedItem as User;
            if (filterItem == null)
                return;
        }

        public void addToConsole(string text)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + text + Environment.NewLine;
                tbConsole.ScrollToEnd();
            }));
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            PointsManager.getInstance().save();
            DatabaseManager.getInstance().close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            c.SendRawMessage("PING tmi.twitch.tv");
            disconnectCheckTimer.Start();
        }

        private void RaffleButton_Click(object sender, RoutedEventArgs e)
        {
            MainGrid.Opacity = 0.5;
            MainGrid.IsEnabled = false;
            var raffle = new RaffleWindow();
            raffle.CloseButton.Click += (a, b) =>
            {
                RaffleManager.getInstance().OnUIClosing();
                OverlayContainer.Visibility = Visibility.Hidden;
                MainGrid.Opacity = 1;
                MainGrid.IsEnabled = true;
            };
            OverlayContainer.Content = raffle;
            OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AutomatedMessagesButton_Start_Click(object sender, RoutedEventArgs e)
        {
            MessagesManager.getInstance().startAllTimers();
            AutomatedMessagesButton_Stop.IsEnabled = true;
            AutomatedMessagesButton_Start.IsEnabled = false;
            updateStatusLabels();
        }

        private void AutomatedMessagesButton_Stop_Click(object sender, RoutedEventArgs e)
        {
            MessagesManager.getInstance().stopAllTimers();
            AutomatedMessagesButton_Stop.IsEnabled = false;
            AutomatedMessagesButton_Start.IsEnabled = true;
            updateStatusLabels();
        }
    }
}
