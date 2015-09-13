using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Model;
using System.Data.SQLite;
using IrcDotNet;
using System.Text.RegularExpressions;

namespace RoxorBot
{
    class FilterManager
    {
        private static FilterManager _instance;
        private List<FilterItem> Filters;

        private FilterManager()
        {
            Logger.Log("Loading FilterManager...");

            MainWindow.ChatMessageReceived += MainWindow_ChatMessageReceived;

            Filters = loadFilters();
            loadAllowedUsers();
        }

        public int getFiltersCount()
        {
            return Filters.Count;
        }

        private List<FilterItem> loadFilters()
        {
            List<FilterItem> result = new List<FilterItem>();
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM filters;");

            while (reader.Read())
            {
                int value;
                if (!int.TryParse((string)reader["duration"], out value))
                    value = 600;

                result.Add(new FilterItem
                {
                    word = (string)reader["word"],
                    duration = value,
                    addedBy = (string)reader["addedBy"],
                    isRegex = (bool)reader["isRegex"],
                    isWhitelist = (bool)reader["isWhitelist"]
                });
            }
            Logger.Log("Loaded " + result.Count + " filters from database.");

            return result;
        }

        private void loadAllowedUsers()
        {
            SQLiteDataReader reader = DatabaseManager.getInstance().executeReader("SELECT * FROM allowedUsers;");

            while (reader.Read())
            {
                var name = (string)reader["name"];
                var isAllowed = (bool)reader["allowed"];

                var user = UsersManager.getInstance().getUser(name);
                if (user == null)
                    user = UsersManager.getInstance().addUser(name, Role.Viewers);

                user.isAllowed = isAllowed;
            }

            Logger.Log("Loaded " + reader.StepCount + " allowed users from database.");
        }

        public bool filterExists(string word)
        {
            return Filters.Any(x => x.word == word);
        }

        public void addFilterWord(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            lock (Filters)
            {
                if (filterExists(word))
                {
                    var filter = getFilter(word);
                    filter.word = word;
                    filter.addedBy = addedBy;
                    filter.duration = banDuration;
                    filter.isRegex = isRegex;
                    filter.isWhitelist = isWhitelist;
                }
                else
                {
                    Filters.Add(new FilterItem { word = word, duration = banDuration, addedBy = addedBy, isRegex = isRegex, isWhitelist = isWhitelist });
                }
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO filters (word, duration, addedBy, isRegex, isWhitelist) VALUES (\"" + word + "\",\"" + banDuration + "\",\"" + addedBy + "\"," + (isRegex ? "1" : "0") + ", " + (isWhitelist ? "1" : "0") + ");");
            }
        }

        public void removeFilterWord(string word)
        {
            lock (Filters)
            {
                Filters.RemoveAll(x => x.word == word);
                DatabaseManager.getInstance().executeNonQuery("DELETE FROM filters WHERE word==\"" + word + "\";");
            }
        }

        private bool checkFilter(IrcRawMessageEventArgs e)
        {
            if (isAdminOrAllowed(e.Message.Source.Name.ToLower()))
                return false;

            var items = Filters.FindAll(x => e.Message.Parameters[1].ToLower().Contains(x.word.ToLower()));
            var exists = (items != null && items.Count > 0);
            if (exists && items.Any(x => x.isWhitelist))
                return false;

            if (!exists)
            {
                var temp = Filters.FindAll(x => x.isRegex);
                var toCheck = new List<FilterItem>();

                foreach (var filter in temp)
                    if (Regex.IsMatch(e.Message.Parameters[1], filter.word))
                        toCheck.Add(filter);

                if (toCheck.Count > 0 && toCheck.Any(x => x.isWhitelist))
                    return false;

                exists = toCheck.Count > 0;
            }
            return exists;
        }

        private bool isAdminOrAllowed(string user)
        {
            return UsersManager.getInstance().isAdmin(user) || UsersManager.getInstance().isAllowed(user);
        }

        public FilterItem getFilter(string text)
        {
            return Filters.Find(x => text.Contains(x.word));
        }

        public List<FilterItem> getAllFilters(FilterMode mode)
        {
            List<FilterItem> result = new List<FilterItem>();
            lock (Filters)
            {
                switch (mode)
                {
                    case FilterMode.All:
                        result.AddRange(Filters.FindAll(x => !x.isWhitelist));
                        break;
                    case FilterMode.Plain:
                        result.AddRange(Filters.FindAll(x => !x.isRegex && !x.isWhitelist));
                        break;
                    case FilterMode.Regex:
                        result.AddRange(Filters.FindAll(x => x.isRegex && !x.isWhitelist));
                        break;
                    case FilterMode.Whitelist:
                        result.AddRange(Filters.FindAll(x => x.isWhitelist));
                        break;
                }
            }
            return result;
        }


        void MainWindow_ChatMessageReceived(object sender, IrcRawMessageEventArgs e)
        {
            if (!(sender is MainWindow))
                return;

            var mainWindow = ((MainWindow)sender);

            if (e.Message.Parameters[1].StartsWith("!addfilter ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string word = commands[1].ToLower();
                int value;

                if (!int.TryParse(commands[2], out value))
                    return;

                if (filterExists(word))
                    return;

                addFilterWord(word, value, e.Message.Source.Name, false, false);

                mainWindow.sendChatMessage(e.Message.Source.Name + ": the word " + word + " was successfully added to database. Reward: " + (value == -1 ? "permanent ban." : value + "s timeout."));
            }
            if (e.Message.Parameters[1].StartsWith("!whitelist ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string word = commands[1].ToLower();

                if (filterExists(word))
                    return;

                addFilterWord(word, 1, e.Message.Source.Name, false, true);

                mainWindow.sendChatMessage(e.Message.Source.Name + ": the word " + word + " is now whitelisted.");
            }
            else if ((e.Message.Parameters[1].StartsWith("!removefilter ") || e.Message.Parameters[1].StartsWith("!removewhitelist ")) && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string word = commands[1].ToLower();

                if (!filterExists(word))
                    return;

                removeFilterWord(word);

                mainWindow.sendChatMessage(e.Message.Source.Name + ": the word " + word + " was successfully removed from database.");
            }
            else if (e.Message.Parameters[1].StartsWith("!allow ") && UsersManager.getInstance().isSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();

                var user = UsersManager.getInstance().getUser(name);
                if (user != null)
                {
                    UsersManager.getInstance().allowUser(name);
                    mainWindow.sendChatMessage(e.Message.Source.Name + ": " + user.Name + " is now allowed.");
                }
            }
            else if (e.Message.Parameters[1].StartsWith("!unallow ") && UsersManager.getInstance().isSuperAdmin(e.Message.Source.Name))
            {
                string[] commands = e.Message.Parameters[1].Split(' ');
                string name = commands[1].ToLower();

                var user = UsersManager.getInstance().getUser(name);
                if (user != null)
                {
                    UsersManager.getInstance().revokeAllowUser(name);
                    mainWindow.sendChatMessage(e.Message.Source.Name + ": Successfully revoked allow from " + user.Name + ".");
                }
            }
            else if (checkFilter(e))
            {
                var item = getFilter(e.Message.Parameters[1]);
                if (item == null)
                {
                    var temp = getAllFilters(FilterMode.Regex);
                    foreach (var filter in temp)
                        if (Regex.IsMatch(e.Message.Parameters[1], filter.word))
                            item = filter;
                }

                if (item == null)
                    return;

                if (item.duration == -1)
                    mainWindow.sendChatMessage(".ban " + e.Message.Source.Name, true);
                else
                    mainWindow.sendChatMessage(".timeout " + e.Message.Source.Name + " " + item.duration, true);
                mainWindow.sendChatMessage(e.Message.Source.Name + " awarded " + (item.duration == -1 ? "permanent ban" : item.duration + "s timeout") + " for filtered word HeyGuys");
                mainWindow.addToConsole(e.Message.Source.Name + " awarded " + (item.duration == -1 ? "permanent ban" : item.duration + "s timeout") + " for filtered word.");
            }
        }

        public static FilterManager getInstance()
        {
            if (_instance == null)
                _instance = new FilterManager();
            return _instance;
        }
    }

    public enum FilterMode
    {
        All,
        Regex,
        Plain,
        Whitelist
    }
}
