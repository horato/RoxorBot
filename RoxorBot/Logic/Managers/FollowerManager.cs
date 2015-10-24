using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FollowerManager;
using System.Net;
using System.Web.Script.Serialization;
using RoxorBot.Model;

namespace RoxorBot
{
    class FollowerManager
    {
        private static FollowerManager _instance;

        private FollowerManager()
        {
            Logger.Log("Initializing FollowerManager...");

            var timer = new System.Timers.Timer(2 * 60 * 1000);
            timer.AutoReset = true;
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                var followers = UsersManager.getInstance().getAllUsers().FindAll(x => x.IsFollower);
                var list = loadFollowers();

                if (list.Count < 1)
                    return;
                if (followers == null)
                {
                    Logger.Log("Loaded " + getFollowersCount() + " followers.");
                    return;
                }

                foreach (var user in list)
                    followers.RemoveAll(x => x.InternalName == user.InternalName);
                foreach (var user in followers)
                {
                    user.IsFollower = false;
                    user.IsFollowerSince = new DateTime(999, 12, 30);
                }

                Logger.Log("Followers updated. Detected total of " + getFollowersCount() + " followers and " + followers.Count + " unfollows.");
            }
            catch
            {
                Logger.Log("Failed to update followers.");
            }
        }

        private List<User> loadFollowers()
        {
            var continueLoading = true;
            var offset = 0;
            var result = new List<User>();

            while (continueLoading)
            {
                Followers_FollowerManager followers;
                using (WebClient client = new WebClient { Encoding = System.Text.Encoding.UTF8 })
                {
                    string json = client.DownloadString("https://api.twitch.tv/kraken/channels/roxork0/follows?direction=DESC&limit=50&offset=" + offset + "&rand=" + Environment.TickCount);
                    followers = new JavaScriptSerializer().Deserialize<Followers_FollowerManager>(json);
                }

                if (followers == null)
                    return new List<User>();

                foreach (var follower in followers.follows)
                {
                    var u = UsersManager.getInstance().addUser(follower.user.display_name, Model.Role.Viewers);
                    u.IsFollower = true;
                    u.IsFollowerSince = TimeParser.GetDuration(follower.created_at);
                    if (u.IsFollowerSince.Year == 999)
                        Logger.Log("Failed to parse following since for user " + u.Name + " in " + "https://api.twitch.tv/kraken/users/" + u.InternalName + "/follows/channels/roxork0.");
                    result.Add(u);
                }

                offset += 50;
                if (followers.follows.Length == 0)
                    continueLoading = false;
            }

            return result;
        }

        public static FollowerManager getInstance()
        {
            if (_instance == null)
                _instance = new FollowerManager();
            return _instance;
        }

        public int getFollowersCount()
        {
            var u = UsersManager.getInstance().getAllUsers().FindAll(x => x.IsFollower);
            if (u == null)
                return 0;
            return u.Count;
        }
    }
}
