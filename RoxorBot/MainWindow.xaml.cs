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

namespace RoxorBot
{
    public partial class MainWindow : MetroWindow
    {
        private IrcClient c;
        private SQLiteConnection dbConnection;
        private List<DateTime> queue;
        private System.Timers.Timer floodTimer;
        private System.Timers.Timer rewardTimer;
        private List<User> users;
        private Dictionary<string, int> points;
        private int timerReward = 108; //per hour
        private List<FilterItem> filters;
        private delegate void ListChanged();
        private event ListChanged OnListChanged;
        private static HttpListener _listener;

        public MainWindow()
        {
            //var x = Regex.Match("http://www.twitch.tv/", @"^(https?://)?([\da-z.-]+).([a-z.]{2,6})([/\w .-]*)*/?$");

            InitializeComponent();

            Disconnect_Button.IsEnabled = false;
            queue = new List<DateTime>();
            users = new List<User>();
            points = new Dictionary<string, int>();
            filters = new List<FilterItem>();
            OnListChanged += MainWindow_OnListChanged;

            initDB();
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:60024/");
            _listener.Start();
            _listener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
        }
        private void ProcessRequest(IAsyncResult result)
        {
            HttpListenerContext context = _listener.EndGetContext(result);
            HttpListenerRequest request = context.Request;

            var sr = new StreamReader(request.InputStream);
            var command = sr.ReadToEnd();

            string responseString;
            if (command != "action=getCommand")
            {
                command = command.Split(new string[] { "data=" }, StringSplitOptions.None)[1];
                System.Diagnostics.Debug.WriteLine(command);
                responseString = "null";
            }
            else
                responseString = "API.getVolume()";

            HttpListenerResponse response = context.Response;
            response.ContentType = "text/html";
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;

            output.Write(buffer, 0, buffer.Length);
            output.Close();
            _listener.BeginGetContext(new AsyncCallback(ProcessRequest), null);
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
                    initUsers(chatters.chatters.staff, Role.Saff);
                    initUsers(chatters.chatters.admins, Role.Admins);
                    initUsers(chatters.chatters.global_mods, Role.Global_mods);
                    initUsers(chatters.chatters.moderators, Role.Moderators);
                    initUsers(chatters.chatters.viewers, Role.Viewers);
                    tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] Loaded " + chatters.chatter_count + " online viewers." + Environment.NewLine;
                }

            }
            catch (Exception ee)
            {
                tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + ee.ToString() + Environment.NewLine;
            }

            new Thread(() =>
            {
                try
                {
                    c = new IrcClient();
                    var connectedEvent = new ManualResetEventSlim(false);
                    IPEndPoint point = new IPEndPoint(Dns.GetHostAddresses("irc.twitch.tv")[0], 80);
                    c.Connected += (sender2, e2) => connectedEvent.Set();
                    c.RawMessageReceived += c_RawMessageReceived;
                    c.RawMessageSent += (arg1, arg2) =>
                    {
                        if (arg2 != null)
                            System.Diagnostics.Debug.WriteLine("sent " + arg2.Message.Command);
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
                        return;
                    }

                    floodTimer = new System.Timers.Timer(1000);
                    floodTimer.AutoReset = true;
                    floodTimer.Elapsed += (arg1, arg2) =>
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                            {
                                List<DateTime> temp = new List<DateTime>();
                                foreach (var item in queue)
                                    temp.Add(item);
                                foreach (var item in temp)
                                    if (item.AddSeconds(30) < DateTime.Now)
                                        queue.Remove(item);
                            }));
                    };
                    floodTimer.Start();

                    c.SendRawMessage("JOIN #roxork0");

                    Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
                    {
                        tbStatus.Text = "Connected";

                        Disconnect_Button.IsEnabled = true;
                    }));
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.ToString());
                    System.Diagnostics.Debug.WriteLine(exc.ToString());
                    Connect_Button.IsEnabled = true;
                }
            }).Start();
        }

        private void Start_Button_OnClick(object sender, RoutedEventArgs e)
        {
            rewardTimer = new System.Timers.Timer(300000);
            rewardTimer.AutoReset = true;
            rewardTimer.Elapsed += (a, b) =>
            {
                foreach (User user in users)
                {
                    if (!points.ContainsKey(user.InternalName))
                        points.Add(user.InternalName, timerReward / 12);
                    else
                        points[user.InternalName] += timerReward / 12;
                }
                foreach (KeyValuePair<string, int> kvp in points)
                    new SQLiteCommand("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + kvp.Key + "\"," + kvp.Value + ");", dbConnection).ExecuteNonQuery();
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] Timer tick. " + users.Count + " users awarded " + timerReward / 12 + " points." + Environment.NewLine;
                }));

            };
            rewardTimer.Start();
            Start_Button.IsEnabled = false;
            Stop_Button.IsEnabled = true;
        }

        private void Stop_Button_OnClick(object sender, RoutedEventArgs e)
        {
            rewardTimer.Stop();
            Start_Button.IsEnabled = true;
            Stop_Button.IsEnabled = false;
        }

        void c_RawMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            System.Diagnostics.Debug.Write("RawMessageReceived: Command: " + e.Message.Command + (e.Message.Source == null ? "" : " From: " + e.Message.Source.Name) + " Parameters: ");
            foreach (string s in e.Message.Parameters)
                if (!string.IsNullOrEmpty(s))
                    System.Diagnostics.Debug.Write(s + ",");
            System.Diagnostics.Debug.WriteLine("");

            if (e.Message.Command == "PRIVMSG" && e.Message.Parameters[0] == "#roxork0")
                handleRawMessage(e);
            else if (e.Message.Command == "JOIN")
                addUser(e.Message.Source.Name, Role.Viewers);
            else if (e.Message.Command == "PART")
                removeUser(e.Message.Source.Name);
            else if (e.Message.Command == "MODE")
                handleMODE(e);
            else if (e.Message.Command == "366" && e.Message.Parameters[2] == "End of /NAMES list")
                sendChatMessage("ItsBoshyTime KAPOW Keepo");
            //else if(e.Message.Command == "PART" && e.Message.Source.Name.ToLower().Contains("roxork0bot"))
            //   c.SendRawMessage("JOIN #roxork0");
        }

        private void handleMODE(IrcRawMessageEventArgs e)
        {
            bool add = e.Message.Parameters[1].ToLower() == "+o";
            string nick = e.Message.Parameters[2];

            User user = users.Find(x => x.InternalName == nick.ToLower());
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
                        }
                        else
                        {
                            DateTime time = TimeParser.GetDuration(followers.created_at);
                            if (time.Month == 999)
                                System.Diagnostics.Debug.WriteLine("Error !since: parsing time: " + "https://api.twitch.tv/kraken/users/" + e.Message.Source.Name + "/follows/channels/roxork0");
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
            else if (e.Message.Parameters[1] == "!points")
            {
                string user = e.Message.Source.Name.ToLower();
                if (points.ContainsKey(user))
                    sendChatMessage(e.Message.Source.Name + ": You have " + points[user] + " points.");
                else
                    sendChatMessage(e.Message.Source.Name + ": You don't have any points.");
            }
            else if (e.Message.Parameters[1].StartsWith("!addpoints ") && e.Message.Source.Name.ToLower() == "roxork0")
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (!points.ContainsKey(name))
                    points.Add(name, value);
                else
                    points[name] += value;

                new SQLiteCommand("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + name + "\"," + points[name] + ");", dbConnection).ExecuteNonQuery();

                sendChatMessage(e.Message.Source.Name + " added " + value + " points to " + name + ". " + name + " now has " + points[name] + " points.");
            }
            else if (e.Message.Parameters[1].StartsWith("!removepoints ") && e.Message.Source.Name.ToLower() == "roxork0")
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (points.ContainsKey(name))
                {
                    if (points[name] < value)
                        points[name] = 0;
                    else
                        points[name] -= value;

                    new SQLiteCommand("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + name + "\"," + points[name] + ");", dbConnection).ExecuteNonQuery();

                    sendChatMessage(e.Message.Source.Name + " subtracted " + value + " points from " + name + ". " + name + " now has " + points[name] + " points.");
                }
            }
            else if (e.Message.Parameters[1].StartsWith("!addfilter ") && users.Any(x => x.InternalName == e.Message.Source.Name.ToLower() && x.Role != Role.Viewers))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string word = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (filters.Any(x => x.word == word))
                    return;

                filters.Add(new FilterItem { word = word, duration = value.ToString(), addedBy = e.Message.Source.Name, isRegex = false });
                new SQLiteCommand("INSERT OR REPLACE INTO filters (word, duration, addedBy, isRegex) VALUES (\"" + word + "\",\"" + value + "\",\"" + e.Message.Source.Name + "\",0);", dbConnection).ExecuteNonQuery();

                sendChatMessage(e.Message.Source.Name + " the word " + word + " was successfully added to database. Reward: " + (value == -1 ? "permanent ban." : value + "s timeout."));
            }
            else if (e.Message.Parameters[1].StartsWith("!removefilter ") && users.Any(x => x.InternalName == e.Message.Source.Name.ToLower() && x.Role != Role.Viewers))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string word = commands[1].ToLower();

                if (!filters.Any(x => x.word == word))
                    return;

                filters.RemoveAll(x => x.word == word);
                new SQLiteCommand("DELETE FROM filters WHERE word==\"" + word + "\";", dbConnection).ExecuteNonQuery();

                sendChatMessage(e.Message.Source.Name + " the word " + word + " was successfully removed from database.");
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
                        var start = TimeParser.GetDuration(stream.created_at);
                        if (start.Month != 999)
                        {
                            var time = DateTime.Now - start;
                            sendChatMessage(string.Format("Streaming for " + (time.Days > 0 ? "{0}d" : "") + " {1}h {2:D2}m {3:D2}s", time.Days, time.Hours, time.Minutes, time.Seconds));
                            return;
                        }
                        System.Diagnostics.Debug.WriteLine("Error !uptime: parsing time: https://api.twitch.tv/kraken/streams?channel=roxork0");
                    }
                }
                catch (Exception ee)
                {
                    System.Diagnostics.Debug.WriteLine(ee.ToString());
                }
            }
            else if (checkFilter(e))
            {
                var item = filters.Find(x => e.Message.Parameters[1].Contains(x.word));
                if (item == null)
                {
                    var temp = filters.FindAll(x => x.isRegex);
                    foreach (var filter in temp)
                        if (Regex.IsMatch(e.Message.Parameters[1], filter.word))
                            item = filter;
                }

                if (item == null)
                    return;

                sendChatMessage(e.Message.Source.Name + " awarded " + (int.Parse(item.duration) == -1 ? "permanent ban" : item.duration + "s timeout") + " for filtered word HeyGuys");
                if (item.duration == "-1")
                    sendChatMessage(".ban " + e.Message.Source.Name);
                else
                    sendChatMessage(".timeout " + e.Message.Source.Name + " " + item.duration);
            }
        }

        private bool checkFilter(IrcRawMessageEventArgs e)
        {
            var exists = filters.Any(x => e.Message.Parameters[1].ToLower().Contains(x.word.ToLower()));
            if (!exists)
            {
                var temp = filters.FindAll(x => x.isRegex);
                foreach (var filter in temp)
                    if (Regex.IsMatch(e.Message.Parameters[1], filter.word))
                        exists = true;
            }
            return exists && users.Any(x => x.InternalName == e.Message.Source.Name.ToLower() && x.Role == Role.Viewers);
        }

        private void sendChatMessage(string message)
        {
            if (queue.Count > 90)
            {
                tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] Queue limit reached. Ignoring: " + message + Environment.NewLine;
                return;
            }
            c.SendRawMessage("PRIVMSG #roxork0 :" + message);
        }
        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            users.Clear();
            Disconnect_Button.IsEnabled = false;
            Connect_Button.IsEnabled = true;
            try
            {
                c.SendRawMessage("PART #roxork0");
                c.Disconnect();
            }
            catch (Exception exc) { MessageBox.Show(exc.ToString()); }
        }

        private void initDB()
        {
            if (!File.Exists("botDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("botDatabase.sqlite");
                dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                dbConnection.Open();
                new SQLiteCommand("CREATE TABLE points (name VARCHAR(64) PRIMARY KEY, score INT);", dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE filters (word TEXT PRIMARY KEY, duration TEXT, addedBy TEXT, isRegex BOOL DEFAULT false);", dbConnection).ExecuteNonQuery();
            }
            else
            {
                dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                dbConnection.Open();
            }

            SQLiteDataReader reader = new SQLiteCommand("SELECT * FROM points;", dbConnection).ExecuteReader();
            while (reader.Read())
                points.Add((string)reader["name"], (int)reader["score"]);

            tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] Loaded " + points.Count + " viewers from database." + Environment.NewLine;

            reader = new SQLiteCommand("SELECT * FROM filters;", dbConnection).ExecuteReader();
            while (reader.Read())
                filters.Add(new FilterItem
                {
                    word = (string)reader["word"],
                    duration = (string)reader["duration"],
                    addedBy = (string)reader["addedBy"],
                    isRegex = (bool)reader["isRegex"]
                });

            tbConsole.Text += "[" + DateTime.Now.ToString("HH:mm:ss") + "] Loaded " + filters.Count + " filtered words from database." + Environment.NewLine;
        }

        private void initUsers(string[] list, Role role)
        {
            foreach (string s in list)
                users.Add(new User { Name = s, InternalName = s.ToLower(), Role = role });
            if (OnListChanged != null)
                OnListChanged();
        }

        private void addUser(string user, Role role)
        {
            if (!users.Any(x => x.InternalName == user.ToLower()))
                users.Add(new User { Name = user, InternalName = user.ToLower(), Role = role });
            if (OnListChanged != null)
                OnListChanged();
        }

        private void removeUser(String user)
        {
            users.RemoveAll(x => x.InternalName == user.ToLower());
            if (OnListChanged != null)
                OnListChanged();
        }

        private void MainWindow_OnListChanged()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                UsersListView.Items.Clear();
                var temp = new List<User>();

                foreach (User u in users)
                    temp.Add(new User { Name = u.Name, InternalName = u.InternalName, Role = u.Role });

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

            MainGrid.Opacity = 0.1;
            MainGrid.IsEnabled = false;
            SettingsGrid.Visibility = Visibility.Visible;
        }

        private void CloseSettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            MainGrid.Opacity = 1;
            MainGrid.IsEnabled = true;
            SettingsGrid.Visibility = Visibility.Hidden;
        }

        private void AddFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.FilterWordBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.DurationBox.Text))
                    return;
                OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                filters.Add(new FilterItem { word = dialog.FilterWordBox.Text, duration = dialog.DurationBox.Text, addedBy = "AdminConsole", isRegex = (bool)dialog.IsRegexCheckBox.IsChecked });
                new SQLiteCommand("INSERT OR REPLACE INTO filters (word, duration, addedBy, isRegex) VALUES (\"" + dialog.FilterWordBox.Text + "\",\"" + dialog.DurationBox.Text + "\", \"AdminConsole\", " + ((bool)dialog.IsRegexCheckBox.IsChecked ? "1" : "0") + ");", dbConnection).ExecuteNonQuery();
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

        private void drawFilters()
        {
            FilterListDataGrid.Items.Clear();
            foreach (FilterItem item in filters)
                FilterListDataGrid.Items.Add(item);
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
                filters.RemoveAll(x => x.word == filterItem.word);
                new SQLiteCommand("DELETE FROM filters WHERE word==\"" + filterItem.word + "\";", dbConnection).ExecuteNonQuery();
                drawFilters();
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            dbConnection.Close();
        }
    }
}
