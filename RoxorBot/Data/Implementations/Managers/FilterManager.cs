using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Prism.Events;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces.Factories.Wrapper;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Interfaces.Repositories;
using RoxorBot.Data.Model.Wrappers;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Implementations.Managers
{
    public class FilterManager : IFilterManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IUsersManager _usersManager;
        private readonly IFilterWrapperFactory _filterWrapperFactory;
        private readonly IFilterRepository _repository;
        private readonly Dictionary<Guid, FilterWrapper> _filters;

        public int FiltersCount => _filters.Count;

        public FilterManager(IEventAggregator aggregator, IUsersManager usersManager, IFilterWrapperFactory filterWrapperFactory, IFilterRepository filterRepository)
        {
            _aggregator = aggregator;
            _usersManager = usersManager;
            _filterWrapperFactory = filterWrapperFactory;
            _repository = filterRepository;
            _aggregator.GetEvent<AddLogEvent>().Publish("Loading FilterManager...");
            _filters = LoadFilters();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + _filters.Count + " filtered words from database.");
        }

        private Dictionary<Guid, FilterWrapper> LoadFilters()
        {
            var filters = _repository.GetAll();
            var result = new Dictionary<Guid, FilterWrapper>();
            foreach (var filter in filters)
                result.Add(filter.Id, _filterWrapperFactory.CreateNew(filter));

            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + result.Count + " filters from database.");
            return result;
        }

        public bool FilterExists(Guid id)
        {
            return _filters.ContainsKey(id);
        }

        public bool FilterExists(string word)
        {
            lock (_filters)
                return _filters.Values.Any(x => x.Word == word);
        }

        public void AddFilterWord(string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            lock (_filters)
            {
                var filter = _filterWrapperFactory.CreateNew(word, banDuration, addedBy, isRegex, isWhitelist);
                _filters.Add(filter.Id, filter);
            }
        }

        public void UpdateFilterWord(Guid id, string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            UpdateModel(id, word, banDuration, addedBy, isRegex, isWhitelist);
            if (!_filters.ContainsKey(id))
                return;

            var filter = _filters[id];
            filter.Word = word;
            filter.Author = addedBy;
            filter.BanDuration = banDuration;
            filter.IsRegex = isRegex;
            filter.IsWhitelist = isWhitelist;
        }

        private void UpdateModel(Guid id, string word, int banDuration, string addedBy, bool isRegex, bool isWhitelist)
        {
            var filter = _repository.FindById(id);
            if (filter == null)
                return;

            filter.Word = word;
            filter.BanDuration = banDuration;
            filter.Author = addedBy;
            filter.IsRegex = isRegex;
            filter.IsWhitelist = isWhitelist;
            _repository.Save(filter);
            _repository.FlushSession();
        }

        public void RemoveFilterWord(FilterWrapper wrapper)
        {
            if (wrapper == null)
                return;

            _repository.Remove(wrapper.Model);
            _repository.FlushSession();
            lock (_filters)
                _filters.Remove(wrapper.Id);
        }

        public void RemoveFilterWord(string word)
        {
            var filter = GetFilter(word);
            if (filter == null)
                return;

            RemoveFilterWord(filter);
        }

        public FilterWrapper CheckFilter(ChatMessage e)
        {
            if (e == null)
                return null;
            if (IsAdminOrAllowed(e.Username))
                return null;

            lock (_filters)
            {
                var items = _filters.Select(x => x.Value).Where(x => e.Message.ToLower().Contains(x.Word.ToLower())).ToList();
                if (items.Any(x => x.IsWhitelist))
                    return null;
                if (items.Any())
                    return items.First();

                var temp = _filters.Select(x => x.Value).Where(x => x.IsRegex);
                var toCheck = temp.Where(x => Regex.IsMatch(e.Message, x.Word)).ToList();
                if (toCheck.Any(x => x.IsWhitelist))
                    return null;

                return toCheck.FirstOrDefault();
            }
        }

        private bool IsAdminOrAllowed(string user)
        {
            return _usersManager.IsAdmin(user) || _usersManager.IsAllowed(user);
        }

        public FilterWrapper GetFilter(Guid id)
        {
            if (!_filters.ContainsKey(id))
                return null;

            return _filters[id];
        }

        public FilterWrapper GetFilter(string word)
        {
            lock (_filters)
                return _filters.Values.FirstOrDefault(x => x.Word == word);
        }

        public List<FilterWrapper> GetAllFilters(FilterMode mode)
        {
            var result = new List<FilterWrapper>();
            lock (_filters)
            {
                switch (mode)
                {
                    case FilterMode.All:
                        result.AddRange(_filters.Values.Where(x => !x.IsWhitelist));
                        break;
                    case FilterMode.Plain:
                        result.AddRange(_filters.Values.Where(x => !x.IsRegex && !x.IsWhitelist));
                        break;
                    case FilterMode.Regex:
                        result.AddRange(_filters.Values.Where(x => x.IsRegex && !x.IsWhitelist));
                        break;
                    case FilterMode.Whitelist:
                        result.AddRange(_filters.Values.Where(x => x.IsWhitelist));
                        break;
                }
            }
            return result;
        }
    }
}
