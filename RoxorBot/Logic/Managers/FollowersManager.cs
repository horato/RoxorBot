using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Model;
using RoxorBot.Data.Model.JSON.FollowerManager;

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

                _logger.Log("Followers updated. Detected total of " + GetFollowersCount() + " followers and " + followers.Count + " unfollows.");
            }
            catch
            {
                _logger.Log("Failed to update followers.");
            }
        }

        private List<User> LoadFollowers()
        {
            var continueLoading = true;
            var offset = 0;
            var result = new List<User>();

            while (continueLoading)
            {
                Followers_FollowerManager followers;
                using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                {
                    var json = client.DownloadData("https://api.twitch.tv/kraken/channels/roxork0/follows?direction=DESC&limit=50&offset=" + offset + "&rand=" + Environment.TickCount);
                    followers = new DataContractJsonSerializer(typeof(Followers_FollowerManager)).ReadObject(new MemoryStream(json)) as Followers_FollowerManager;
                }

                if (followers == null)
                    return new List<User>();

                foreach (var follower in followers.Follows)
                {
                    var u = _usersManager.AddOrGetUser(follower.User.DisplayName, Role.Viewers);
                    u.IsFollower = true;
                    u.IsFollowerSince = TimeParser.GetDuration(follower.CreatedAt);
                    if (u.IsFollowerSince == null)
                        _logger.Log("Failed to parse following since for user " + u.Name + " in " + "https://api.twitch.tv/kraken/users/" + u.InternalName + "/follows/channels/roxork0.");
                    result.Add(u);
                }

                offset += 50;
                if (followers.Follows.Length == 0)
                    continueLoading = false;
            }

            return result;
        }

        public int GetFollowersCount()
        {
            var u = _usersManager.GetAllUsers().FindAll(x => x.IsFollower);
            return u.Count;
        }
    }
}
