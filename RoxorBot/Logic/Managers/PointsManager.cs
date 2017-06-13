using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            LoadViewers();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + GetUsersCount() + " viewers from database.");
        }

        public void AddPoints(string user, int points)
        {
            SetPoints(user, GetPointsForUser(user) + points);
        }

        public void RemovePoints(string user, int points)
        {
            if (!UserExists(user))
                return;

            if (GetPointsForUser(user) < points)
                SetPoints(user, 0);
            else
                SetPoints(user, GetPointsForUser(user) - points);
        }

        public void SetPoints(string user, int points, bool dbUpdate = true)
        {
            var u = _usersManager.GetUser(user);
            if (u == null)
                u = _usersManager.AddOrGetUser(user, Model.Role.Viewers);

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
