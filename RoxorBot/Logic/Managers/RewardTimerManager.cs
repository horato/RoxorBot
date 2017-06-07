using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Logic.Managers
{
    public class RewardTimerManager : IRewardTimerManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;
        private readonly IPointsManager _pointsManager;

        private readonly System.Timers.Timer _rewardTimer;
        public bool IsRunning => _rewardTimer.Enabled;
        public bool IsPaused { get; private set; }

        public RewardTimerManager(IEventAggregator aggregator, IUsersManager usersManager, IPointsManager pointsManager)
        {
            _aggregator = aggregator;
            _usersManager = usersManager;
            _pointsManager = pointsManager;
            _rewardTimer = new System.Timers.Timer(5 * 60 * 1000);
            _rewardTimer.AutoReset = true;
            _rewardTimer.Elapsed += _rewardTimer_Elapsed;
            _aggregator.GetEvent<ChatConnectionChangedEvent>().Subscribe(OnChatConnectionChanged);
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
                if (!user.IsFollower || !user.isOnline)
                    continue;
                if (user.RewardTimer < 25)
                {
                    user.RewardTimer += 5;
                }
                else
                {
                    user.RewardTimer = 0;
                    _pointsManager.AddPoints(user.InternalName, Properties.Settings.Default.timerReward);
                    Whispers.sendPrivateMessage(user.InternalName, "You were awarded " + Properties.Settings.Default.timerReward + " points for staying with us another 30 minutes.");
                }
            }
            _aggregator.GetEvent<AddLogEvent>().Publish("Timer tick.");
        }

        public void Start()
        {
            _rewardTimer.Start();
            IsPaused = false;
        }

        public void Stop()
        {
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
