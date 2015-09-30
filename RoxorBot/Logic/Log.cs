using RoxorBot.Model.Youtube;
using System;
using System.IO;
using System.Runtime.ExceptionServices;

namespace RoxorBot
{
    /// <summary>
    /// Shamelessly stolen from https://github.com/LegendaryClient/LegendaryClient LOL
    /// </summary>
    public static class Logger
    {
        private static MainWindow _mainWindow;
        public static MainWindow mainWindow
        {
            get
            {
                return _mainWindow;
            }
            set
            {
                if (_mainWindow != null)
                    return;
                _mainWindow = value;
            }
        }

        public static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (e.Exception is VideoParseException)
                return;
            if (e.Exception.Message.Contains("does not contain a definition"))
                return;

            WriteToLog.Log("A first chance exception was thrown", "EXCEPTION");
            WriteToLog.Log(e.Exception.Message, "EXCEPTION");
            WriteToLog.Log(e.ToString(), "EXCEPTION");

        }

        public static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs x)
        {
            WriteToLog.Log("An unhandled exception was thrown", "UNHANDLEDEXCEPTION");
            var ex = (Exception)x.ExceptionObject;
            WriteToLog.Log(ex.Message, "UNHANDLEDEXCEPTION");
            WriteToLog.Log(ex.ToString(), "UNHANDLEDEXCEPTION");
        }

        public static void Log(string line, string type = "LOG")
        {
            mainWindow.addToConsole(line);
            WriteToLog.Log(line, type);
        }
    }

    public static class WriteToLog
    {
        public static string ExecutingDirectory;
        public static string LogfileName;

        public static void Log(string lines, string type = "LOG")
        {
            using (FileStream stream = File.Open(Path.Combine(ExecutingDirectory, "Logs", LogfileName), FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (var file = new StreamWriter(stream))
                file.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type.ToUpper(), lines);
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
