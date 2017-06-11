using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Model;

namespace RoxorBot.Data.Interfaces
{
    public interface IChatManager
    {
        bool IsConnecting { get; }
        bool IsConnected { get; }
        int FloodQueueCount { get; }
        void Connect();
        void Disconnect();
        void SendChatMessage(string message, bool overrideFloodQueue = false);
        void TimeoutUser(string user, TimeSpan duration);
        void BanUser(string user);
    }
}
