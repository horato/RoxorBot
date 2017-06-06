using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class PointsManager : IPointsManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly IUsersManager _usersManager;

        public PointsManager(IEventAggregator aggregator, IChatManager chatManager, IDatabaseManager databaseManager, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _chatManager = chatManager;
            _databaseManager = databaseManager;
            _usersManager = usersManager;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing PointsManager...");
            _aggregator.GetEvent<IrcMessageReceived>().Subscribe(ChatMessageReceived);
            LoadViewers();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + GetUsersCount() + " viewers from database.");
        }

        private void ChatMessageReceived(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count < 2)
                return;

            var msg = e.Message.Parameters[1];
            if (msg.StartsWith("!points"))
            {
                string[] commands = msg.Split(' ');
                if (commands.Length < 2)
                    _chatManager.SendChatMessage(e.Message.Source.Name + ": You have " + GetPointsForUser(e.Message.Source.Name) + " points.");
                else
                    _chatManager.SendChatMessage(e.Message.Source.Name + ": " + commands[1] + " has " + GetPointsForUser(commands[1]) + " points.");
            }
            else if (msg.StartsWith("!addpoints ") && _usersManager.IsSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = msg.Split(' ');
                if (commands.Length < 3)
                    return;

                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                AddPoints(name, value);
                _chatManager.SendChatMessage(e.Message.Source.Name + ": Added " + value + " points to " + name + ".");
            }
            else if (msg.StartsWith("!removepoints ") && _usersManager.IsSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = msg.Split(' ');
                if (commands.Length < 3)
                    return;

                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (UserExists(name))
                {
                    RemovePoints(name, value);

                    _chatManager.SendChatMessage(e.Message.Source.Name + " Subtracted " + value + " points from " + name + ". " + name + " now has " + GetPointsForUser(name) + " points.");
                }
            }
        }

        public void AddPoints(string user, int points)
        {
            SetPoints(user, GetPointsForUser(user) + points);
        }

        public void RemovePoints(string user, int points)
        {
            if (GetPointsForUser(user) < points)
                SetPoints(user, 0);
            else
                SetPoints(user, GetPointsForUser(user) - points);
        }

        public void SetPoints(string user, int points, bool dbUpdate = true)
        {
            var u = _usersManager.GetUser(user);
            if (u == null)
                u = _usersManager.AddUser(user, Model.Role.Viewers);

            u.Points = points;

            if (dbUpdate)
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + user.ToLower() + "\"," + GetPointsForUser(user) + ");");
        }

        public bool UserExists(string name)
        {
            return (_usersManager.GetUser(name) != null);
        }

        public int GetPointsForUser(string name)
        {
            var user = _usersManager.GetUser(name);
            if (user == null)
                return 0;
            else
                return user.Points;
        }

        public int GetUsersCount()
        {
            return _usersManager.UsersCount;
        }

        public void Save()
        {
            var users = _usersManager.GetAllUsers();
            foreach (var user in users)
                if (user.Points > 0)
                    _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + user.InternalName + "\"," + user.Points + ");");
        }

        private void LoadViewers()
        {
            var reader = _databaseManager.ExecuteReader("SELECT * FROM points;");

            while (reader.Read())
                SetPoints((string)reader["name"], (int)reader["score"], false);
        }
    }
}
