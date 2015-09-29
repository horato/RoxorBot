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
        private Dictionary<int, AutomatedMessage> messages;
        private MainWindow mainWindow;
        private bool isRunning;

        private MessagesManager()
        {
            Logger.Log("Initializing MessagesManager...");
            messages = loadMessages();
            isRunning = false;
            Logger.Log("Loaded " + getMessagesCount() + " messages from database.");
        }

        public static MessagesManager getInstance()
        {
            if (_instance == null)
                _instance = new MessagesManager();
            return _instance;
        }

        public void addAutomatedMessage(string msg, int interval, bool start, bool enabled, int id = 0)
        {
            if (id > 0)
            {
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO messages (id, message, interval, enabled) VALUES (" + id + ", '" + msg + "', " + interval + ", " + (enabled ? "1" : "0") + ");");
            }
            else
            {
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO messages (message, interval, enabled) VALUES ('" + msg + "', " + interval + ", 1);");
                var reader = DatabaseManager.getInstance().executeReader("SELECT last_insert_rowid()");
                if (!reader.Read())
                    return;
                id = reader.GetInt32(0);
            }
            var message = getMessage(id);
            if (message == null)
            {
                message = new AutomatedMessage();
                message.id = id;
                message.active = true;
                messages.Add(id, message);
            }
            message.message = msg;
            message.interval = interval;

            if (message.timer != null && message.timer.Enabled)
                message.timer.Stop();
            message.timer = new System.Timers.Timer(message.interval * 60 * 1000);
            message.timer.AutoReset = true;
            message.timer.Elapsed += (a, b) =>
            {
                if (isRunning && message.active)
                    mainWindow.sendChatMessage(message.message);
            };
            if (start)
                message.timer.Start();
        }

        public void removeMessage(AutomatedMessage msg)
        {
            lock (messages)
            {
                if (msg.timer != null)
                    msg.timer.Stop();
                messages.Remove(msg.id);
                DatabaseManager.getInstance().executeNonQuery("DELETE FROM messages WHERE id=" + msg.id + ";");
            }
        }

        private Dictionary<int, AutomatedMessage> loadMessages()
        {
            Dictionary<int, AutomatedMessage> result = new Dictionary<int, AutomatedMessage>();
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM messages;");

            while (reader.Read())
            {
                var id = Convert.ToInt32(reader["id"]);
                var msg = (string)reader["message"];
                var interval = (int)reader["interval"];
                var enabled = (bool)reader["enabled"];
                var timer = new System.Timers.Timer(interval * 60 * 1000);
                timer.AutoReset = true;
                timer.Elapsed += (a, b) =>
                {
                    var message = getMessage(id);
                    if (isRunning && message.active)
                        mainWindow.sendChatMessage(message.message);
                };
                result.Add(id, new AutomatedMessage
                {
                    id = id,
                    message = msg,
                    interval = interval,
                    timer = timer,
                    active = enabled
                });
            }

            Logger.Log("Loaded " + result.Count + " automated messages from database.");

            return result;
        }

        /// <summary>
        /// Don't forget to change button's enabled status!
        /// </summary>
        public void startAllTimers()
        {
            foreach (var msg in messages.Values)
                if (msg.timer != null)
                    msg.timer.Start();
            isRunning = true;
        }

        /// <summary>
        /// Don't forget to change button's enabled status!
        /// </summary>
        public void stopAllTimers()
        {
            foreach (var msg in messages.Values)
                if (msg.timer != null)
                    msg.timer.Stop();
            isRunning = false;
        }

        public List<AutomatedMessage> getAllMessages()
        {
            lock (messages)
                return messages.Values.ToList();
        }

        public AutomatedMessage getMessage(int id)
        {
            lock (messages)
            {
                if (messages.ContainsKey(id))
                    return messages[id];
                else
                    return null;
            }
        }

        public int getMessagesCount()
        {
            return messages.Count;
        }

        public bool isActive()
        {
            return isRunning;
        }

        internal void setReference(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }
    }
}
