using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Extensions;
using RoxorBot.Data.Logic;
using TwitchLib;

namespace RoxorBot.Data.Implementations
{
    public static class TwitchHelper
    {
        public static void EnsureTwitchLoginCorrect()
        {
            var isValid = false;
            while (!isValid)
            {
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.twitch_oauth))
                    Properties.Settings.Default.twitch_oauth = Prompt.ShowDialog("Specify twitch oauth", "Twitch oauth");
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.TwitchId))
                    Properties.Settings.Default.TwitchId = Prompt.ShowDialog("Specify twitch clientId", "Twitch client Id");

                TwitchAPI.Settings.ClientId = Properties.Settings.Default.TwitchId;
                TwitchAPI.Settings.AccessToken = Properties.Settings.Default.twitch_oauth.Replace("oauth:", "");

                var root = TwitchAPI.Root.v5.GetRoot().WaitAndReturn();
                if (root?.Token == null || !root.Token.Valid)
                {
                    Properties.Settings.Default.twitch_oauth = string.Empty;
                    Properties.Settings.Default.TwitchId = string.Empty;
                    continue;
                }

                Properties.Settings.Default.Save();
                isValid = true;
            }
        }
    }
}
