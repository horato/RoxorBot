using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;

namespace RoxorBot.Data.Interfaces
{
    public interface IAutomatedMessagesManager
    {
        void AddAutomatedMessage(string msg, int interval, bool start, bool enabled, int id = 0);
        void RemoveMessage(AutomatedMessage msg);
        void StartAllTimers();
        void StopAllTimers();
        void PauseAllTimers();
        List<AutomatedMessage> GetAllMessages();
        AutomatedMessage GetMessage(int id);
        bool IsPaused { get; }
        bool IsRunning { get; }
    }
}
