using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Twitch.Chat;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib.Models.Client;

namespace RoxorBot.Logic.Managers
{
    public class RaffleManager : IRaffleManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IPointsManager _pointsManager;
        private readonly IUsersManager _usersManager;

        public int PointsRequired
        {
            get { return _entryPointsRequired; }
            set
            {
                if (IsRunning)
                    return;

                _entryPointsRequired = value;
            }
        }

        public bool IsFollowersOnly
        {
            get { return _isFollowersOnly; }
            set
            {
                if (IsRunning)
                    return;

                _isFollowersOnly = value;
            }
        }
        public string RaffleName { get; set; }
        public string AcceptedWords => string.Join(";", _acceptedWords);
        public bool IsRunning { get; private set; }
        public event EventHandler<UserWrapper> OnUserAdd;
        public event EventHandler OnWinnerPicked;

        
        private bool _isFollowersOnly;
        private int _entryPointsRequired = 100;
        private readonly List<string> _acceptedWords = new List<string> { "!raffle", "!join" };
        private readonly List<UserWrapper> _users = new List<UserWrapper>();
        private bool _winnerSelected;

        public RaffleManager(IEventAggregator aggregator, IChatManager chatManager, IPointsManager pointsManager, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;

            _aggregator.GetEvent<ChatMessageReceivedEvent>().Subscribe(OnChatMessageReceived);
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing RaffleManager...");
        }

        private void OnChatMessageReceived(ChatMessage e)
        {
            if (e == null)
                return;
            if (!IsRunning || string.Equals(e.Username, Properties.Settings.Default.twitch_login, StringComparison.CurrentCultureIgnoreCase))
                return;
            if (!_acceptedWords.Contains(e.Message))
                return;

            var user = _usersManager.GetUser(e.Username);
            if (user == null)
                return;
            if (_users.Contains(user))
                return;
            if (_isFollowersOnly && !user.IsFollower)
                return;
            if (_pointsManager.GetPointsForUser(user.ValueName) < _entryPointsRequired)
                return;

            _pointsManager.RemovePoints(user.ValueName, _entryPointsRequired);
            lock (_users)
                _users.Add(user);

            if (OnUserAdd != null)
                OnUserAdd(this, user);

            Whispers.sendPrivateMessage(user.ValueName, "You are now participating in the raffle. You have " + user.Points + " points remaining.");
        }

        public void StartRaffle()
        {
            IsRunning = true;
            _winnerSelected = false;
            var sb = new StringBuilder();
            foreach (var v in _acceptedWords)
                sb.Append(v + " ");
            _chatManager.SendChatMessage("Raffle " + RaffleName + " started with a " + _entryPointsRequired + " points " + (_isFollowersOnly ? "and followers only" : "") + " entry requirement. Accepted word(s) is/are: " + sb.ToString());
        }

        public void StopRaffle()
        {
            IsRunning = false;
            _chatManager.SendChatMessage("Raffle ended with " + _users.Count + " participating users. Wait for streamer to announce results.");
        }

        public void PickWinner()
        {
            if (_winnerSelected || _users.Count < 1 || IsRunning)
                return;

            lock (_users)
            {
                if (_isFollowersOnly)
                    _users.RemoveAll(x => !x.IsFollower);

                Random rnd = new Random();
                int num = rnd.Next(0, _users.Count - 1);
                var winner = _users[num];
                _chatManager.SendChatMessage("Winner is " + winner.VisibleName + ". Random number selected from interval <0," + (_users.Count - 1) + "> was " + num + ". Congratulations.");
                _winnerSelected = true;
                _users.Clear();
                if (OnWinnerPicked != null)
                    OnWinnerPicked(this, null);
            }
        }

        public void OnUIClosing()
        {
            IsRunning = false;
            if (!_winnerSelected && _users.Count > 0)
            {
                lock (_users)
                {
                    _chatManager.SendChatMessage("Raffle canceled. Refunding " + _entryPointsRequired + " points to " + _users.Count + " users.");
                    foreach (var user in _users)
                        _pointsManager.AddPoints(user.ValueName, _entryPointsRequired);
                }
            }
            _winnerSelected = false;
            RaffleName = "";
            _users.Clear();
        }


        public void SetAcceptedWords(string words)
        {
            if (IsRunning)
                return;

            _acceptedWords.Clear();
            if (string.IsNullOrEmpty(words))
                return;

            var split = words.Split(';');
            foreach (var s in split)
                _acceptedWords.Add(s);
        }

        public IEnumerable<UserWrapper> GetAllParticipants()
        {
            return _users.ToList();
        }
    }
}
