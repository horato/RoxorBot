using System;

namespace RoxorBot.Data.Implementations.Logging
{
    public interface ILogger
    {
        void Debug(object message);
        void Debug(string format, params object[] args);

        void Info(object message);
        void InfoFormat(string format, params object[] args);

        void Warning(object message);
        void WarningFormat(string format, params object[] args);

        void Error(object message);
        void ErrorFormat(string format, params object[] args);
        void Error(Exception ex);
        void Error(object message, Exception ex);

        void Fatal(object message);
        void FatalFormat(string format, params object[] args);
        void Fatal(Exception ex);
        void Fatal(object message, Exception ex);
    }
}
