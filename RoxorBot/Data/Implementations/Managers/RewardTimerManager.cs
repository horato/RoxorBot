using System;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Logic;

namespace RoxorBot.Data.Implementations.Managers
{
    public class RewardTimerManager : IRewardTimerManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;
        private readonly IPointsManager _pointsManager;
        private bool _isInitialized;

        private System.Timers.Timer _rewardTimer;
        public bool IsRunning => _rewardTimer?.Enabled ?? false;
        public bool IsPaused { get; private set; }

        public RewardTimerManager(IEventAggregator aggregator, IUsersManager usersManager, IPointsManager pointsManager)
        {
            _aggregator = aggregator;
            _usersManager = usersManager;
            _pointsManager = pointsManager;
        }

        public void Init()
        {
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing RewardTimerManager...");
            _rewardTimer = new System.Timers.Timer(5 * 60 * 1000);
            _rewardTimer.AutoReset = true;
            _rewardTimer.Elapsed += _rewardTimer_Elapsed;
            _aggregator.GetEvent<ChatConnectionChangedEvent>().Subscribe(OnChatConnectionChanged);
            _isInitialized = true;
        }

        private void OnChatConnectionChanged(ChatConnectionState obj)
        {
            if (obj != ChatConnectionState.Connected)
                return;

            if (IsPaused)
                Start();
        }

        private void _rewardTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var users = _usersManager.GetAllUsers();

            foreach (var user in users)
            {
                if (!user.IsFollower || !user.IsOnline)
                    continue;
                if (user.RewardTimer < 25)
                {
                    user.RewardTimer += 5;
                }
                else
                {
                    user.RewardTimer = 0;
                    _pointsManager.AddPoints(user.ValueName, Properties.Settings.Default.timerReward);
                    Whispers.sendPrivateMessage(user.ValueName, "You were awarded " + Properties.Settings.Default.timerReward + " points for staying with us another 30 minutes.");
                }
            }
            _aggregator.GetEvent<AddLogEvent>().Publish("Timer tick.");
        }

        public void Start()
        {
            if(!_isInitialized)
                throw new InvalidOperationException($"{nameof(RewardTimerManager)} is not initialized.");

            _rewardTimer.Start();
            IsPaused = false;
        }

        public void Stop()
        {
            if (!_isInitialized)
                throw new InvalidOperationException($"{nameof(RewardTimerManager)} is not initialized.");

            _rewardTimer.Stop();
            IsPaused = false;
        }

        public void Pause()
        {
            Stop();
            IsPaused = true;
        }
    }
}
