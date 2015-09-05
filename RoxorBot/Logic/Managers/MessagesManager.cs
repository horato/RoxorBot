using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class MessagesManager
    {
        private static MessagesManager _instance;
        private Dictionary<string, AutomatedMessage> messages;
        private MainWindow mainWindow;

        private MessagesManager()
        {
            Logger.Log("Initializing MessagesManager...");
            messages = loadFilters();
        }

        public static MessagesManager getInstance()
        {
            if (_instance == null)
                _instance = new MessagesManager();
            return _instance;
        }

        public void addFilterWord(string msg, int interval, bool start)
        {
            var message = new AutomatedMessage();
            message.message = msg;
            message.interval = interval;
            message.timer = new System.Timers.Timer(interval*60*1000);
            fillTimer(message.timer, msg);
            if(start)
                message.timer.Start();
            DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO messages (message, interval) VALUES ('" + msg + "', " + interval + ");");
            messages.Add(msg, message);
        }

        private void fillTimer(System.Timers.Timer timer, string message)
        {
            timer.AutoReset = true;
            timer.Elapsed += (a, b) =>
            {
                mainWindow.sendChatMessage(message);
            };
        }

        public void removeMessage(AutomatedMessage msg)
        {
            if (msg.timer != null)
                msg.timer.Stop();
            messages.Remove(msg.message);
            DatabaseManager.getInstance().executeNonQuery("DELETE FROM messages WHERE message='" + msg.message + "';");
        }
        private Dictionary<string, AutomatedMessage> loadFilters()
        {
            Dictionary<string, AutomatedMessage> result = new Dictionary<string, AutomatedMessage>();
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM messages;");

            while (reader.Read())
            {
                var msg = (string)reader["message"];
                var interval = (int)reader["interval"];
                var timer = new System.Timers.Timer(interval*60*1000);
                fillTimer(timer, msg);
                result.Add(msg, new AutomatedMessage
                {
                    message = msg,
                    interval = interval,
                    timer = timer
                });
            }

            Logger.Log("Loaded " + result.Count + " filters from database.");

            return result;
        }

        public void startAllTimers()
        {
            foreach (var msg in messages.Values)
                if (msg.timer != null)
                    msg.timer.Start();
        }
        public void stoptAllTimers()
        {
            foreach (var msg in messages.Values)
                if (msg.timer != null)
                    msg.timer.Stop();
        }

        public List<AutomatedMessage> getAllMessages()
        {
            return messages.Values.ToList();
        }

        public int getMessagesCount()
        {
            return messages.Count;
        }

        internal void setReference(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
