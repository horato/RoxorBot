using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RoxorBot
{
    static class Whispers
    {
        private static IrcClient c;
        private static bool connected = false;

        public static void connect()
        {
            new Thread(() =>
            {
                try
                {
                    c = new IrcClient();
                    var connectedEvent = new ManualResetEventSlim(false);
                    IPEndPoint point = new IPEndPoint(Dns.GetHostAddresses("199.9.253.119")[0], 6667); //whispers
                    c.Connected += (sender2, e2) => connectedEvent.Set();

                    c.Connect(point, false, new IrcUserRegistrationInfo()
                    {
                        UserName = Properties.Settings.Default.twitch_login,
                        NickName = Properties.Settings.Default.twitch_login,
                        Password = Properties.Settings.Default.twitch_oauth
                    });
                    if (!connectedEvent.Wait(10000))
                    {
                        c.Dispose();
                        System.Diagnostics.Debug.WriteLine("timed out");
                        return;
                    }

                    c.SendRawMessage("CAP REQ :twitch.tv/membership");
                    c.SendRawMessage("CAP REQ :twitch.tv/commands");

                    //c.SendRawMessage("JOIN #roxork0");
                    connected = true;

                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.ToString());
                }
            }).Start();
        }
        public static void sendPrivateMessage(string user, string message)
        {
            c.SendRawMessage("PRIVMSG #jtv :/w " + user + " " + message);
        }
    }
}
