using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class RaffleManager
    {
        private static RaffleManager _instance;
        private MainWindow mainWindow;
        private bool isFollowersOnly;
        private int entryPointsRequired;
        private bool isRunning;
        private List<string> acceptedWords;
        private List<User> users;
        private bool winnerSelected;
        private string raffleName;
        public event EventHandler<User> OnUserAdd;
        public event EventHandler OnWinnerPicked;

        private RaffleManager()
        {
            Logger.Log("Initializing RaffleManager...");
            entryPointsRequired = 100;
            isFollowersOnly = false;
            isRunning = false;
            winnerSelected = false;
            raffleName = "";
            acceptedWords = new List<string>() { "!raffle", "!join" };
            users = new List<User>();

            MainWindow.ChatMessageReceived += mainWindow_ChatMessageReceived;
        }

        private void mainWindow_ChatMessageReceived(object sender, IrcDotNet.IrcRawMessageEventArgs e)
        {
            if (!isRunning || (e.Message.Source.Name.ToLower() == Properties.Settings.Default.twitch_login.ToLower()))
                return;
            if (e.Message.Parameters.Count < 2)
                return;
            var msg = e.Message.Parameters[1];

            if (acceptedWords.Contains(msg))
            {
                var user = UsersManager.getInstance().getUser(e.Message.Source.Name);

                if (user == null)
                    return;
                if (users.Contains(user))
                    return;
                if (isFollowersOnly && !user.IsFollower)
                    return;
                if (PointsManager.getInstance().getPointsForUser(user.InternalName) < entryPointsRequired)
                    return;

                PointsManager.getInstance().removePoints(user.InternalName, entryPointsRequired);
                lock (users)
                    users.Add(user);

                if (OnUserAdd != null)
                    OnUserAdd(this, user);

                Whispers.sendPrivateMessage(user.InternalName, "You are now participating in the raffle. You have " + user.Points + " points remaining.");
            }
        }

        public void setReference(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public static RaffleManager getInstance()
        {
            if (_instance == null)
                _instance = new RaffleManager();
            return _instance;
        }

        internal void StartRaffle()
        {
            isRunning = true;
            winnerSelected = false;
            var sb = new StringBuilder();
            lock (acceptedWords)
                foreach (var v in acceptedWords)
                    sb.Append(v + " ");
            mainWindow.sendChatMessage("Raffle " + raffleName + " started with a " + entryPointsRequired + " points " + (isFollowersOnly ? "and followers only" : "") + " entry requirement. Accepted word(s) is/are: " + sb.ToString());
        }

        internal void StopRaffle()
        {
            isRunning = false;
            mainWindow.sendChatMessage("Raffle ended with " + users.Count + " participating users. Wait for streamer to announce results.");
        }

        internal void PickWinner()
        {
            if (winnerSelected || users.Count < 1 || isRunning)
                return;

            lock (users)
            {
                if (isFollowersOnly)
                    users.RemoveAll(x => !x.IsFollower);

                Random rnd = new Random();
                int num = rnd.Next(0, users.Count - 1);
                var winner = users[num];
                mainWindow.sendChatMessage("Winner is " + winner.Name + ". Random number selected from interval <0," + (users.Count - 1) + "> was " + num + ". Congratulations.");
                winnerSelected = true;
                users.Clear();
                if (OnWinnerPicked != null)
                    OnWinnerPicked(this, null);
            }
        }

        internal void setPointsRequired(int points)
        {
            if (isRunning)
                return;

            entryPointsRequired = points;
        }

        internal void setFollowersOnly(bool followersOnly)
        {
            if (isRunning)
                return;

            isFollowersOnly = followersOnly;
        }

        internal void OnUIClosing()
        {
            isRunning = false;
            if (!winnerSelected && users.Count > 0)
            {
                lock (users)
                {
                    mainWindow.sendChatMessage("Raffle canceled. Refunding " + entryPointsRequired + " points to " + users.Count + " users.");
                    foreach (var user in users)
                        PointsManager.getInstance().addPoints(user.InternalName, entryPointsRequired);
                }
            }
            winnerSelected = false;
            raffleName = "";
            users.Clear();
        }

        public void setRaffleName(string name)
        {
            raffleName = name;
        }

        public void setAcceptedWords(string words)
        {
            acceptedWords.Clear();
            if (string.IsNullOrEmpty(words))
                return;

            var split = words.Split(';');
            foreach (var s in split)
                acceptedWords.Add(s);
        }
    }
}
