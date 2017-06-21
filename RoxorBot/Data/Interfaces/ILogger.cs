using RoxorBot.Data.Enums;

namespace RoxorBot.Data.Interfaces
{
    public interface ILogger
    {
        void Log(string line, LogType type = LogType.Info);
    }
}
