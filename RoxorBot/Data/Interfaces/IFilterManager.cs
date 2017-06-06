using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Model;

namespace RoxorBot.Data.Interfaces
{
    public interface IFilterManager
    {
        int FiltersCount { get; }
        bool FilterExists(int id);
        bool FilterExists(string word);
        void AddFilterWord(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist, int id = 0);
        void RemoveFilterWord(int id);
        void RemoveFilterWord(string word);
        FilterItem GetFilter(int id);
        FilterItem GetFilter(string word);
        List<FilterItem> GetAllFilters(FilterMode mode);
    }
}
