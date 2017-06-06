using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations
{
    /// <summary>
    /// Shamelessly stolen from https://github.com/LegendaryClient/LegendaryClient LOL
    /// </summary>
    public class Logger : ILogger
    {
        public Logger()
        {
            WriteToLog.ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WriteToLog.LogfileName = "RoxorBot.Log";
            WriteToLog.CreateLogFile();
        }

        public void Log(string line, LogType type = LogType.Info)
        {
            WriteToLog.Log(line, type);
        }

        private static class WriteToLog
        {
            public static string ExecutingDirectory;
            public static string LogfileName;

            public static void Log(string lines, LogType type)
            {
                //TODO: Log4net
                if (lines == null)
                    return;

                try
                {
                    var path = Path.Combine(ExecutingDirectory, "Logs", LogfileName);
                    var text = $"({DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}) [{type.ToString().ToUpper()}] {lines}";
                    File.WriteAllText(path, text);
                }
                catch
                {
                    //
                }
            }

            public static void CreateLogFile()
            {
                //Generate A Unique file to use as a log file
                if (!Directory.Exists(Path.Combine(ExecutingDirectory, "Logs")))
                    Directory.CreateDirectory(Path.Combine(ExecutingDirectory, "Logs"));
                LogfileName = string.Format("{0}T{1} {2}", DateTime.Now.ToShortDateString().Replace("/", "_"),
                    DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "-"), "_" + LogfileName);
                FileStream file = File.Create(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
                file.Close();
            }
        }
    }
}
