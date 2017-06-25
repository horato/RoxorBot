using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Managers;

namespace RoxorBot.Data.Implementations.Managers
{
    public class ManagerLoader : IManagersLoader
    {
        private readonly IEventAggregator _aggregator;
        private readonly IAutomatedMessagesManager _messagesManager;
        private readonly IFilterManager _filterManager;
        private readonly IFollowersManager _followersManager;
        private readonly IChatManager _chatManager;
        private readonly IPointsManager _pointsManager;
        private readonly IRaffleManager _raffleManager;
        private readonly IRewardTimerManager _rewardTimerManager;
        private readonly IUserCommandsManager _userCommandsManager;
        private readonly IUsersManager _usersManager;
        private readonly IYoutubeManager _youtubeManager;
        private readonly IChatMessageHandler _chatMessageHandler;

        public ManagerLoader(IEventAggregator aggregator, IAutomatedMessagesManager messagesManager, IFilterManager filterManager, IFollowersManager followersManager, IChatManager chatManager, IPointsManager pointsManager, IRaffleManager raffleManager, IRewardTimerManager rewardTimerManager, IUserCommandsManager userCommandsManager, IUsersManager usersManager, IYoutubeManager youtubeManager, IChatMessageHandler chatMessageHandler)
        {
            _aggregator = aggregator;
            _messagesManager = messagesManager;
            _filterManager = filterManager;
            _followersManager = followersManager;
            _chatManager = chatManager;
            _pointsManager = pointsManager;
            _raffleManager = raffleManager;
            _rewardTimerManager = rewardTimerManager;
            _userCommandsManager = userCommandsManager;
            _usersManager = usersManager;
            _youtubeManager = youtubeManager;
            _chatMessageHandler = chatMessageHandler;
        }

        public void Load()
        {
            _aggregator.GetEvent<AddLogEvent>().Publish("Program init...");
            _usersManager.Init();
            _messagesManager.Init();
            _filterManager.Init();
            _chatManager.Init();
            _pointsManager.Init();
            _raffleManager.Init();
            _rewardTimerManager.Init();
            _userCommandsManager.Init();
            _chatMessageHandler.Init();
            Task.Factory.StartNew(_followersManager.Init);
            Task.Factory.StartNew(_youtubeManager.Init);
            _aggregator.GetEvent<AddLogEvent>().Publish("Program init finished. Keep in mind that followers and backup playlist are still loading!");
        }
    }
}
