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
