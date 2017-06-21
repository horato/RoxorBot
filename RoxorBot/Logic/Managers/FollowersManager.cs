using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.JSON.FollowerManager;
using TwitchLib;
using TwitchLib.Models.API.v3.Follows;

namespace RoxorBot.Logic.Managers
{
    public class FollowersManager : IFollowersManager
    {
        private readonly ILogger _logger;
        private readonly IUsersManager _usersManager;

        public FollowersManager(ILogger logger, IUsersManager usersManager)
        {
            _logger = logger;
            _usersManager = usersManager;
            _logger.Log("Initializing FollowersManager...");

            var timer = new System.Timers.Timer(2 * 60 * 1000);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var followers = _usersManager.GetAllUsers().FindAll(x => x.IsFollower);
                var list = LoadFollowers();
                if (list.Count < 1)
                    return;

                foreach (var user in list)
                    followers.RemoveAll(x => x.InternalName == user.InternalName);
                foreach (var user in followers)
                {
                    user.IsFollower = false;
                    user.IsFollowerSince = null;
                }

                _usersManager.SaveAll();
                _logger.Log("Followers updated. Detected total of " + GetFollowersCount() + " followers and " + followers.Count + " unfollows.");
            }
            catch
            {
                _logger.Log("Failed to update followers.");
            }
        }

        private List<UserWrapper> LoadFollowers()
        {
            var continueLoading = true;
            var offset = 0;
            var result = new List<UserWrapper>();

            while (continueLoading)
            {
                var followers = TwitchAPI.Follows.v3.GetFollowers("roxork0", 50, offset).Result;
                if (followers == null)
                    return new List<UserWrapper>();

                foreach (var follower in followers.Followers)
                {
                    var u = _usersManager.AddOrGetUser(follower.User.DisplayName, Role.Viewers);
                    u.IsFollower = true;
                    u.IsFollowerSince = follower.CreatedAt;
                    if (u.IsFollowerSince == null)
                        _logger.Log("Failed to parse following since for user " + u.Name + " in " + "https://api.twitch.tv/kraken/users/" + u.InternalName + "/follows/channels/roxork0.");
                    result.Add(u);
                }

                offset += 50;
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
