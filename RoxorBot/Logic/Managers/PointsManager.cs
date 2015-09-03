using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class PointsManager
    {
        private static PointsManager _instance;
        private Dictionary<string, int> Points;

        private PointsManager()
        {
            Logger.Log("Initializing PointsManager...");
            MainWindow.ChatMessageReceived += MainWindow_ChatMessageReceived;
            Points = loadViewers();
        }

        void MainWindow_ChatMessageReceived(object sender, IrcDotNet.IrcRawMessageEventArgs e)
        {
            if (!(sender is MainWindow))
                return;

            var mainWindow = ((MainWindow)sender);

            if (e.Message.Parameters[1] == "!points")
            {
                string user = e.Message.Source.Name;
                mainWindow.sendChatMessage(user + ": You have " + getPointsForUser(user) + " points.");
            }
            else if (e.Message.Parameters[1].StartsWith("!addpoints ") && e.Message.Source.Name.ToLower() == "roxork0")
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                addPoints(name, value);
            }
            else if (e.Message.Parameters[1].StartsWith("!removepoints ") && e.Message.Source.Name.ToLower() == "roxork0")
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (userExists(name))
                {
                    removePoints(name, value);

                    mainWindow.sendChatMessage(e.Message.Source.Name + " subtracted " + value + " points from " + name + ". " + name + " now has " + PointsManager.getInstance().getPointsForUser(name) + " points.");
                }
            }
        }

        public void addPoints(string user, int points)
        {
            setPoints(user, getPointsForUser(user) + points);
        }

        public void removePoints(string user, int points)
        {
            if (getPointsForUser(user) < points)
                setPoints(user, 0);
            else
                setPoints(user, getPointsForUser(user) - points);
        }

        public void setPoints(string user, int points)
        {
            if (!userExists(user))
                Points.Add(user.ToLower(), points);
            else
                Points[user.ToLower()] = points;

            DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + user.ToLower() + "\"," + Points[user.ToLower()] + ");");
        }

        public bool userExists(string user)
        {
            return Points.ContainsKey(user.ToLower());
        }

        public int getPointsForUser(string user)
        {
            if (userExists(user))
                return Points[user.ToLower()];
            else
                return 0;
        }

        public int getUsersCount()
        {
            return Points.Count;
        }

        public void save()
        {
            foreach (KeyValuePair<string, int> kvp in Points)
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + kvp.Key + "\"," + kvp.Value + ");");
        }

        private Dictionary<string, int> loadViewers()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM points;");

            while (reader.Read())
                result.Add((string)reader["name"], (int)reader["score"]);

            Logger.Log("Loaded " + result.Count + " viewers from database.");

            return result;
        }

        public static PointsManager getInstance()
        {
            if (_instance == null)
                _instance = new PointsManager();
            return _instance;
        }
    }
}
