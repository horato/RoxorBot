using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib;

namespace RoxorBot.Data.Implementations.Managers
{
    public class FollowersManager : IFollowersManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;
        private readonly Timer _updateTimer;

        public FollowersManager(IUsersManager usersManager, IEventAggregator aggregator)
        {
            _usersManager = usersManager;
            _aggregator = aggregator;
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing FollowersManager...");

            _updateTimer = new Timer(2 * 60 * 1000);
            _updateTimer.AutoReset = false;
            _updateTimer.Elapsed += timer_Elapsed;

            //TODO: init on new thread
            Task.Factory.StartNew(LoadFollowers);
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            LoadFollowers();
        }

        private void LoadFollowers()
        {
            _updateTimer.Stop();
            try
            {
                var followers = _usersManager.GetAllUsers().FindAll(x => x.IsFollower);
                var list = GetFollowersFromTwitch();
                if (list.Count < 1)
                    return;

                foreach (var user in list)
                    followers.RemoveAll(x => x.ValueName == user.ValueName);
                foreach (var user in followers)
                {
                    user.IsFollower = false;
                    user.IsFollowerSince = null;
                }

                _usersManager.SaveAll();
                _aggregator.GetEvent<AddLogEvent>().Publish("Followers updated. Detected total of " + GetFollowersCount() + " followers and " + followers.Count + " unfollows.");
            }
            catch
            {
                _aggregator.GetEvent<AddLogEvent>().Publish("Failed to update followers.");
            }
            _updateTimer.Start();
        }

        private List<UserWrapper> GetFollowersFromTwitch()
        {
            var continueLoading = true;
            var offset = 0;
            var result = new List<UserWrapper>();

            while (continueLoading)
            {
                var followers = TwitchAPI.Follows.v3.GetFollowers("roxork0", 100, offset).Result;
                if (followers == null)
                    return new List<UserWrapper>();

                var newFollowers = followers.Followers.Where(x => _usersManager.GetUser(x.User.DisplayName) == null);
                _usersManager.AddNewUsers(newFollowers.Select(x => x.User.DisplayName), Role.Viewers);

                foreach (var follower in followers.Followers)
                {
                    try
                    {
                        var u = _usersManager.AddOrGetUser(follower.User.DisplayName, Role.Viewers);
                        u.IsFollower = true;
                        u.IsFollowerSince = follower.CreatedAt;
                        result.Add(u);
                    }
                    catch
                    {
                        //
                    }
                }

                offset += 100;
                if (followers.Followers.Length == 0)
                    continueLoading = false;
            }

            _usersManager.SaveAll();
            return result;
        }

        public int GetFollowersCount()
        {
            var u = _usersManager.GetAllUsers().FindAll(x => x.IsFollower);
            return u.Count;
        }
    }
}
