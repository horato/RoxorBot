using System;
using System.Diagnostics;
using log4net;

namespace RoxorBot.Data.Implementations.Logging
{
    public class Logger : ILogger
    {
        private readonly ILog _log;

        public Logger(ILog log)
        {
            _log = log;
        }


        public void Debug(object message)
        {
            if (_log.IsDebugEnabled)
                _log.Debug(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void Debug(string format, params object[] args)
        {
            if (_log.IsDebugEnabled)
                _log.DebugFormat(AppendCallingMethodName(format), args);
        }

        public void Info(object message)
        {
            if (_log.IsInfoEnabled)
                _log.Info(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_log.IsInfoEnabled)
                _log.InfoFormat(AppendCallingMethodName(format), args);
        }

        public void Warning(object message)
        {
            if (_log.IsWarnEnabled)
                _log.Warn(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void WarningFormat(string format, params object[] args)
        {
            if (_log.IsWarnEnabled)
                _log.WarnFormat(AppendCallingMethodName(format), args);
        }

        public void Error(object message)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_log.IsErrorEnabled)
                _log.ErrorFormat(AppendCallingMethodName(format), args);
        }

        public void Error(Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(ex?.Message ?? string.Empty), ex);
        }

        public void Error(object message, Exception ex)
        {
            if (_log.IsErrorEnabled)
                _log.Error(AppendCallingMethodName(message?.ToString() ?? string.Empty), ex);
        }

        public void Fatal(object message)
        {
            if (_log.IsFatalEnabled)
                _log.Fatal(AppendCallingMethodName(message?.ToString() ?? string.Empty));
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_log.IsFatalEnabled)
                _log.FatalFormat(AppendCallingMethodName(format), args);
        }

        public void Fatal(Exception ex)
        {
            if (_log.IsFatalEnabled)
                _log.Fatal(AppendCallingMethodName(ex?.Message ?? string.Empty), ex);
        }

        public void Fatal(object message, Exception ex)
        {
            if (_log.IsFatalEnabled)
                _log.Fatal(AppendCallingMethodName(message?.ToString() ?? string.Empty), ex);
        }

        private string AppendCallingMethodName(string message)
        {
            var stackTrace = new StackTrace();
            if (stackTrace.FrameCount < 3)
                return message;

            try
            {
                var methodBase = stackTrace.GetFrame(2).GetMethod();
                if (methodBase.ReflectedType.FullName == _log.Logger.Name)
                    return $".{methodBase.Name} : {message}";

                return $"{methodBase.ReflectedType.FullName}.{methodBase.Name} : {message}";
            }
            catch (Exception ex)
            {
                _log.Error($"{GetType().FullName}.AppendCallingMethodName] - Chyba pri zapisu do logu.", ex);
            }

            return message;
        }
    }
}
