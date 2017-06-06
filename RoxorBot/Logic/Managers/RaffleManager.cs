﻿using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class RaffleManager : IRaffleManager
    {
        private readonly ILogger _logger;
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
        public event EventHandler<User> OnUserAdd;
        public event EventHandler OnWinnerPicked;



        private bool _isFollowersOnly;
        private int _entryPointsRequired = 100;
        private readonly List<string> _acceptedWords = new List<string> { "!raffle", "!join" };
        private readonly List<User> _users = new List<User>();
        private bool _winnerSelected;

        public RaffleManager(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IPointsManager pointsManager, IUsersManager usersManager)
        {
            _logger = logger;
            _aggregator = aggregator;
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _usersManager = usersManager;

            _aggregator.GetEvent<IrcMessageReceived>().Subscribe(OnIrcMessageReceived);
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing RaffleManager...");
        }

        private void OnIrcMessageReceived(IrcRawMessageEventArgs obj)
        {
            var e = obj;
            if (e == null)
                return;
            if (!IsRunning || (e.Message.Source.Name.ToLower() == Properties.Settings.Default.twitch_login.ToLower()))
                return;
            if (e.Message.Parameters.Count < 2)
                return;
            var msg = e.Message.Parameters[1];

            if (_acceptedWords.Contains(msg))
            {
                var user = _usersManager.GetUser(e.Message.Source.Name);

                if (user == null)
                    return;
                if (_users.Contains(user))
                    return;
                if (_isFollowersOnly && !user.IsFollower)
                    return;
                if (_pointsManager.GetPointsForUser(user.InternalName) < _entryPointsRequired)
                    return;

                _pointsManager.RemovePoints(user.InternalName, _entryPointsRequired);
                lock (_users)
                    _users.Add(user);

                if (OnUserAdd != null)
                    OnUserAdd(this, user);

                Whispers.sendPrivateMessage(user.InternalName, "You are now participating in the raffle. You have " + user.Points + " points remaining.");
            }
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
                _chatManager.SendChatMessage("Winner is " + winner.Name + ". Random number selected from interval <0," + (_users.Count - 1) + "> was " + num + ". Congratulations.");
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
                        _pointsManager.AddPoints(user.InternalName, _entryPointsRequired);
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

        public IEnumerable<User> GetAllParticipants()
        {
            return _users.ToList();
        }
    }
}
