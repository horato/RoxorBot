using System;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class AddFilterDialogViewModel : BindableBase, IDialogViewModel
    {
        private readonly IFilterManager _filterManager;
        private string _filterWord;
        private int _banDuration;
        private bool _isRegex;
        private bool _isWhitelist;
        private FilterWrapper _selectedFilter;

        public Action Close { get; set; }
        public string FilterWord { get { return _filterWord; } set { _filterWord = value; RaisePropertyChanged(); } }
        public int BanDuration { get { return _banDuration; } set { _banDuration = value; RaisePropertyChanged(); } }
        public bool IsRegex { get { return _isRegex; } set { _isRegex = value; RaisePropertyChanged(); } }
        public bool IsWhitelist { get { return _isWhitelist; } set { _isWhitelist = value; RaisePropertyChanged(); } }

        public AddFilterDialogViewModel(IFilterManager filterManager)
        {
            _filterManager = filterManager;
        }

        public void SetData(object data)
        {
            var filter = data as FilterWrapper;
            if (filter == null)
                return;

            _selectedFilter = filter;
            FilterWord = _selectedFilter.Word;
            BanDuration = _selectedFilter.BanDuration;
            IsRegex = _selectedFilter.IsRegex;
            IsWhitelist = _selectedFilter.IsWhitelist;
        }

        [Command]
        public void Add()
        {
            if (string.IsNullOrWhiteSpace(FilterWord))
                return;
            if (BanDuration < -1 || BanDuration == 0)
                return;

            if (_selectedFilter == null)
                _filterManager.AddFilterWord(FilterWord, BanDuration, "AdminConsole", IsRegex, IsWhitelist);
            else
                _filterManager.UpdateFilterWord(_selectedFilter.Id, FilterWord, BanDuration, "AdminConsole", IsRegex, IsWhitelist);

            Close?.Invoke();
        }
    }
}
