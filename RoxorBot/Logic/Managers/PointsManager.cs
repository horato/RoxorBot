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

        private PointsManager()
        {
            Logger.Log("Initializing PointsManager...");
            MainWindow.ChatMessageReceived += MainWindow_ChatMessageReceived;
            loadViewers();
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
            else if (e.Message.Parameters[1].StartsWith("!addpoints ") && UsersManager.getInstance().isSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                addPoints(name, value);
                mainWindow.sendChatMessage(e.Message.Source.Name + ": Added " + value + " points to " + name + ".");
            }
            else if (e.Message.Parameters[1].StartsWith("!removepoints ") && UsersManager.getInstance().isSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (userExists(name))
                {
                    removePoints(name, value);

                    mainWindow.sendChatMessage(e.Message.Source.Name + " Subtracted " + value + " points from " + name + ". " + name + " now has " + getPointsForUser(name) + " points.");
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

        public void setPoints(string user, int points, bool dbUpdate = true)
        {
            var u = UsersManager.getInstance().getUser(user);
            if (u == null)
                u = UsersManager.getInstance().addUser(user, Model.Role.Viewers);

            u.Points = points;

            if (dbUpdate)
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + user.ToLower() + "\"," + getPointsForUser(user) + ");");
        }

        public bool userExists(string name)
        {
            return (UsersManager.getInstance().getUser(name) != null);
        }

        public int getPointsForUser(string name)
        {
            var user = UsersManager.getInstance().getUser(name);
            if (user == null)
                return 0;
            else
                return user.Points;
        }

        public int getUsersCount()
        {
            return UsersManager.getInstance().getUsersCount();
        }

        public void save()
        {
            var users = UsersManager.getInstance().getAllUsers();
            foreach (var user in users)
                if (user.Points > 0)
                    DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO points (name, score) VALUES (\"" + user.InternalName + "\"," + user.Points + ");");
        }

        private void loadViewers()
        {
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM points;");

            while (reader.Read())
                setPoints((string)reader["name"], (int)reader["score"], false);

            Logger.Log("Loaded " + getUsersCount() + " viewers from database.");
        }

        public static PointsManager getInstance()
        {
            if (_instance == null)
                _instance = new PointsManager();
            return _instance;
        }
    }
}
