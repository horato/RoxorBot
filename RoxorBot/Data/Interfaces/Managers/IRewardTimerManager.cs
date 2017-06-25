namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IRewardTimerManager : IManagerBase
    {
        void Start();
        void Stop();
        void Pause();
        bool IsRunning { get; }
        bool IsPaused { get; }
    }
}
