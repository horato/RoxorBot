using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Model;
using System.Data.SQLite;
using IrcDotNet;
using System.Text.RegularExpressions;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;
using TwitchLib.Models.Client;

namespace RoxorBot
{
    public class FilterManager : IFilterManager
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _aggregator;
        private readonly IChatManager _chatManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly IUsersManager _usersManager;
        private readonly List<FilterItem> _filters;

        public int FiltersCount => _filters.Count;

        public FilterManager(ILogger logger, IEventAggregator aggregator, IChatManager chatManager, IDatabaseManager databaseManager, IUsersManager usersManager)
        {
            _logger = logger;
            _aggregator = aggregator;
            _chatManager = chatManager;
            _databaseManager = databaseManager;
            _usersManager = usersManager;

            _aggregator.GetEvent<AddLogEvent>().Publish("Loading FilterManager...");
            _filters = LoadFilters();
            LoadAllowedUsers();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + _filters.Count + " filtered words from database.");
        }

        private List<FilterItem> LoadFilters()
        {
            var result = new List<FilterItem>();
            var reader = _databaseManager.ExecuteReader("SELECT * FROM filters;");

            while (reader.Read())
            {
                int value;
                if (!int.TryParse((string)reader["duration"], out value))
                    value = 600;

                result.Add(new FilterItem
                {
                    id = Convert.ToInt32(reader["id"]),
                    word = (string)reader["word"],
                    duration = value,
                    addedBy = (string)reader["addedBy"],
                    isRegex = (bool)reader["isRegex"],
                    isWhitelist = (bool)reader["isWhitelist"]
                });
            }
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + result.Count + " filters from database.");

            return result;
        }

        private void LoadAllowedUsers()
        {
            var reader = _databaseManager.ExecuteReader("SELECT * FROM allowedUsers;");

            while (reader.Read())
            {
                var name = (string)reader["name"];
                var isAllowed = (bool)reader["allowed"];

                var user = _usersManager.GetUser(name);
                if (user == null)
                    user = _usersManager.AddOrGetUser(name, Role.Viewers);

                user.IsAllowed = isAllowed;
            }

            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + reader.StepCount + " allowed users from database.");
        }

        public bool FilterExists(int id)
        {
            return _filters.Any(x => x.id == id);
        }

        public bool FilterExists(string word)
        {
            return _filters.Any(x => x.word == word);
        }

        public void AddFilterWord(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist, int id = 0)
        {
            if (id > 0)
            {
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO filters (id, word, duration, addedBy, isRegex, isWhitelist) VALUES (" + id + ", \"" + word + "\",\"" + banDuration + "\",\"" + addedBy + "\"," + (isRegex ? "1" : "0") + ", " + (isWhitelist ? "1" : "0") + ");");
            }
            else
            {
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO filters (word, duration, addedBy, isRegex, isWhitelist) VALUES (\"" + word + "\",\"" + banDuration + "\",\"" + addedBy + "\"," + (isRegex ? "1" : "0") + ", " + (isWhitelist ? "1" : "0") + ");");
                var reader = _databaseManager.ExecuteReader("SELECT last_insert_rowid()");
                if (!reader.Read())
                    return;
                id = reader.GetInt32(0);
            }
            lock (_filters)
            {
                if (FilterExists(id))
                {
                    var filter = GetFilter(id);
                    filter.word = word;
                    filter.addedBy = addedBy;
                    filter.duration = banDuration;
                    filter.isRegex = isRegex;
                    filter.isWhitelist = isWhitelist;
                }
                else
                {
                    _filters.Add(new FilterItem { id = id, word = word, duration = banDuration, addedBy = addedBy, isRegex = isRegex, isWhitelist = isWhitelist });
                }
            }
        }

        public void RemoveFilterWord(int id)
        {
            lock (_filters)
            {
                _filters.RemoveAll(x => x.id == id);
                _databaseManager.ExecuteNonQuery("DELETE FROM filters WHERE id=" + id + ";");
            }
        }

        public void RemoveFilterWord(string word)
        {
            var filter = GetFilter(word);
            if (filter == null)
                return;

            RemoveFilterWord(filter.id);
        }

        public bool CheckFilter(ChatMessage e)
        {
            if (e == null)
                return false;
            if (IsAdminOrAllowed(e.Username))
                return false;

            var items = _filters.FindAll(x => e.Message.ToLower().Contains(x.word.ToLower()));
            var exists = items.Count > 0;
            if (exists && items.Any(x => x.isWhitelist))
                return false;
            if (exists)
                return true;

            var temp = _filters.FindAll(x => x.isRegex);
            var toCheck = new List<FilterItem>();
            foreach (var filter in temp)
                if (Regex.IsMatch(e.Message, filter.word))
                    toCheck.Add(filter);

            if (toCheck.Count > 0 && toCheck.Any(x => x.isWhitelist))
                return false;

            return toCheck.Count > 0;
        }

        private bool IsAdminOrAllowed(string user)
        {
            return _usersManager.IsAdmin(user) || _usersManager.IsAllowed(user);
        }

        public FilterItem GetFilter(int id)
        {
            return _filters.Find(x => x.id == id);
        }

        public FilterItem GetFilter(string word)
        {
            return _filters.Find(x => x.word == word);
        }

        public List<FilterItem> GetAllFilters(FilterMode mode)
        {
            var result = new List<FilterItem>();
            lock (_filters)
            {
                switch (mode)
                {
                    case FilterMode.All:
                        result.AddRange(_filters.FindAll(x => !x.isWhitelist));
                        break;
                    case FilterMode.Plain:
                        result.AddRange(_filters.FindAll(x => !x.isRegex && !x.isWhitelist));
                        break;
                    case FilterMode.Regex:
                        result.AddRange(_filters.FindAll(x => x.isRegex && !x.isWhitelist));
                        break;
                    case FilterMode.Whitelist:
                        result.AddRange(_filters.FindAll(x => x.isWhitelist));
                        break;
                }
            }
            return result;
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
