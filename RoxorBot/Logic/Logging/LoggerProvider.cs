using System;
using System.Diagnostics;
using log4net;

namespace RoxorBot.Logic.Logging
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
