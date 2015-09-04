using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FollowerManager;
using System.Net;
using System.Web.Script.Serialization;

namespace RoxorBot
{
    class FollowerManager
    {
        private static FollowerManager _instance;

        private FollowerManager()
        {
            Logger.Log("Initializing FollowerManager...");

            init();
        }

        private void init()
        {
            var continueLoading = true;
            var offset = 0;

            while (continueLoading)
            {
                Followers_FollowerManager followers;
                using (WebClient client = new WebClient())
                {
                    string json = client.DownloadString("https://api.twitch.tv/kraken/channels/roxork0/follows?direction=DESC&limit=50&offset=" + offset);
                    followers = new JavaScriptSerializer().Deserialize<Followers_FollowerManager>(json);
                }

                if (followers == null)
                    return;

                foreach (var follower in followers.follows)
                {
                    var u = UsersManager.getInstance().addUser(follower.user.display_name, Model.Role.Viewers);
                    u.IsFollower = true;
                }

                offset += 50;
                if (followers.follows.Length == 0)
                    continueLoading = false;
            }
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
