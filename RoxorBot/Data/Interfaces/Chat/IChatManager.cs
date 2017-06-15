using System;

namespace RoxorBot.Data.Interfaces.Chat
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
