using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Interfaces
{
    public interface IRewardTimerManager
    {
        void Start();
        void Stop();
        void Pause();
        bool IsRunning { get; }
        bool IsPaused { get; }
    }
}
