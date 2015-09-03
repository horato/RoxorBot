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

            Filters = loadFilters();
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
                result.Add(new FilterItem
                {
                    word = (string)reader["word"],
                    duration = (string)reader["duration"],
                    addedBy = (string)reader["addedBy"],
                    isRegex = (bool)reader["isRegex"]
                });

            Logger.Log("Loaded " + result.Count + " filters from database.");

            return result;
        }

        public bool filterExists(string word)
        {
            return Filters.Any(x => x.word == word);
        }

        public void addFilterWord(string word, int banDuration, string addedBy, bool isRegex)
        {
            Filters.Add(new FilterItem { word = word, duration = banDuration.ToString(), addedBy = addedBy, isRegex = isRegex });
            DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO filters (word, duration, addedBy, isRegex) VALUES (\"" + word + "\",\"" + banDuration + "\",\"" + addedBy + "\"," + (isRegex ? "1" : "0") + ");");
        }

        public void removeFilterWord(string word)
        {
            Filters.RemoveAll(x => x.word == word);
            DatabaseManager.getInstance().executeNonQuery("DELETE FROM filters WHERE word==\"" + word + "\";");
        }

        public bool checkFilter(IrcRawMessageEventArgs e)
        {
            var exists = Filters.Any(x => e.Message.Parameters[1].ToLower().Contains(x.word.ToLower()));
            if (!exists)
            {
                var temp = Filters.FindAll(x => x.isRegex);
                foreach (var filter in temp)
                    if (Regex.IsMatch(e.Message.Parameters[1], filter.word))
                        exists = true;
            }
            return exists && !UsersManager.getInstance().isAdmin(e.Message.Source.Name.ToLower());
        }

        public FilterItem getFilter(string text)
        {
            return Filters.Find(x => text.Contains(x.word));
        }

        public List<FilterItem> getAllFilters(FilterMode mode)
        {
            List<FilterItem> result = new List<FilterItem>();

            switch (mode)
            {
                case FilterMode.All:
                    result.AddRange(Filters);
                    break;
                case FilterMode.Plain:
                    result.AddRange(Filters.FindAll(x => !x.isRegex));
                    break;
                case FilterMode.Regex:
                    result.AddRange(Filters.FindAll(x => x.isRegex));
                    break;
            }
            return result;
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
        Plain
    }
}
