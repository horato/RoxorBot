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
        void AddAutomatedMessage(string msg, int interval, bool start, bool enabled);
        void UpdateAutomatedMessage(Guid id, string msg, int interval, bool start, bool enabled);
        void RemoveMessage(AutomatedMessageWrapper msg);
        void StartAllTimers();
        void StopAllTimers();
        void PauseAllTimers();
        List<AutomatedMessageWrapper> GetAllMessages();
        AutomatedMessageWrapper GetMessage(Guid id);
        bool IsPaused { get; }
        bool IsRunning { get; }
    }
}
