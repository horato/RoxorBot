using System;
using System.Collections.Generic;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IFilterManager
    {
        int FiltersCount { get; }
        bool FilterExists(Guid id);
        bool FilterExists(string word);
        void AddFilterWord(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
        void UpdateFilterWord(Guid id, string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist);
        void RemoveFilterWord(FilterWrapper id);
        void RemoveFilterWord(string word);
        FilterWrapper GetFilter(Guid id);
        FilterWrapper GetFilter(string word);
        List<FilterWrapper> GetAllFilters(FilterMode mode);
        FilterWrapper CheckFilter(ChatMessage e);
    }
}
