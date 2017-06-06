using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class AutomatedMessagesManager : IAutomatedMessagesManager
    {
        private readonly ILogger _logger;
        private readonly IChatManager _chatManager;
        private readonly IDatabaseManager _databaseManager;

        private readonly Dictionary<int, AutomatedMessage> _messages;
        public bool IsPaused { get; private set; }
        public bool IsRunning { get; private set; }

        private AutomatedMessagesManager(ILogger logger, IChatManager chatManager, IDatabaseManager databaseManager)
        {
            _logger = logger;
            _chatManager = chatManager;
            _databaseManager = databaseManager;

            logger.Log("Initializing MessagesManager...");
            _messages = LoadMessages();
            IsRunning = false;
        }

        public void AddAutomatedMessage(string msg, int interval, bool start, bool enabled, int id = 0)
        {
            if (id > 0)
            {
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO messages (id, message, interval, enabled) VALUES (" + id + ", '" + msg + "', " + interval + ", " + (enabled ? "1" : "0") + ");");
            }
            else
            {
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO messages (message, interval, enabled) VALUES ('" + msg + "', " + interval + ", 1);");
                var reader = _databaseManager.ExecuteReader("SELECT last_insert_rowid()");
                if (!reader.Read())
                    return;
                id = reader.GetInt32(0);
            }
            var message = GetMessage(id);
            if (message == null)
            {
                message = new AutomatedMessage();
                message.id = id;
                message.active = true;
                _messages.Add(id, message);
            }
            message.message = msg;
            message.interval = interval;

            if (message.timer != null && message.timer.Enabled)
                message.timer.Stop();
            message.timer = new System.Timers.Timer(message.interval * 60 * 1000);
            message.timer.AutoReset = true;
            message.timer.Elapsed += (a, b) =>
            {
                if (IsRunning && message.active)
                    _chatManager.SendChatMessage(message.message);
            };
            if (start)
                message.timer.Start();
        }

        public void RemoveMessage(AutomatedMessage msg)
        {
            lock (_messages)
            {
                msg.timer?.Stop();
                _messages.Remove(msg.id);
                _databaseManager.ExecuteNonQuery("DELETE FROM messages WHERE id=" + msg.id + ";");
            }
        }

        private Dictionary<int, AutomatedMessage> LoadMessages()
        {
            var result = new Dictionary<int, AutomatedMessage>();
            var reader = _databaseManager.ExecuteReader("SELECT * FROM messages;");

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
                    var message = GetMessage(id);
                    if (IsRunning && message.active)
                        _chatManager.SendChatMessage(message.message);
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

            _logger.Log("Loaded " + result.Count + " automated messages from database.");

            return result;
        }

        /// <summary>
        /// Don't forget to change button's enabled status!
        /// </summary>
        public void StartAllTimers()
        {
            foreach (var msg in _messages.Values)
                msg.timer?.Start();

            IsRunning = true;
            IsPaused = false;
        }

        /// <summary>
        /// Don't forget to change button's enabled status!
        /// </summary>
        public void StopAllTimers()
        {
            foreach (var msg in _messages.Values)
                msg.timer?.Stop();

            IsRunning = false;
            IsPaused = false;
        }

        public void PauseAllTimers()
        {
            StopAllTimers();
            IsPaused = true;
        }

        public List<AutomatedMessage> GetAllMessages()
        {
            return _messages.Values.ToList();
        }

        public AutomatedMessage GetMessage(int id)
        {
            if (_messages.ContainsKey(id))
                return _messages[id];

            return null;
        }
    }
}
