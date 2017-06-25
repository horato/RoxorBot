using System.Diagnostics;
using log4net;

namespace RoxorBot.Data.Implementations.Logging
{
    public static class LoggerProvider
    {
        static LoggerProvider()
        {
            log4net.Config.XmlConfigurator.Configure();
        }

        public static ILogger GetLogger()
        {
            var callerType = new StackTrace().GetFrame(1).GetMethod().DeclaringType;
            return new Logger(LogManager.GetLogger(callerType));
        }
    }
}
