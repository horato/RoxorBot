using System.Collections.ObjectModel;
using System.Linq;
using Prism.Mvvm;
using RoxorBot.Controls;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Model.Wrappers;
using RoxorBot.Logic;
using RoxorBot.Modules.Main.Views;
using AddUserDialog = RoxorBot.Modules.Main.Views.AddUserDialog;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class SettingsPageViewModel : BindableBase
    {
        private readonly IAutomatedMessagesManager _automatedMessagesManager;
        private readonly IChatManager _chatManager;
        private readonly ILogger _logger;
        private readonly IFilterManager _filterManager;
        private readonly IUsersManager _usersManager;
        private readonly IUserCommandsManager _userCommandsManager;
        private readonly IPointsManager _pointsManager;
        private readonly IDialogService _dialogService;

        public ObservableCollection<AutomatedMessageWrapper> AutomatedMessages { get; } = new ObservableCollection<AutomatedMessageWrapper>();
        public ObservableCollection<FilterWrapper> Filters { get; } = new ObservableCollection<FilterWrapper>();
        public ObservableCollection<FilterWrapper> Whitelist { get; } = new ObservableCollection<FilterWrapper>();
        public ObservableCollection<UserWrapper> Users { get; } = new ObservableCollection<UserWrapper>();
        public ObservableCollection<UserCommandWrapper> CustomCommands { get; } = new ObservableCollection<UserCommandWrapper>();

        public AutomatedMessageWrapper SelectedAutomatedMessage { get; set; }
        public FilterWrapper SelectedFilter { get; set; }
        public UserWrapper SelectedUser { get; set; }
        public UserCommandWrapper SelectedCustomCommand { get; set; }

        public SettingsPageViewModel(IAutomatedMessagesManager automatedMessagesManager, IChatManager chatManager, ILogger logger, IFilterManager filterManager, IUsersManager usersManager, IUserCommandsManager userCommandsManager, IPointsManager pointsManager, IDialogService dialogService)
        {
            _automatedMessagesManager = automatedMessagesManager;
            _chatManager = chatManager;
            _logger = logger;
            _filterManager = filterManager;
            _usersManager = usersManager;
            _userCommandsManager = userCommandsManager;
            _pointsManager = pointsManager;
            _dialogService = dialogService;

            InitFilters();
            InitAutomatedMessages();
            InitWhitelist();
            InitUsers();
            InitCommands();
        }

        private void InitAutomatedMessages()
        {
            AutomatedMessages.Clear();
            var msgs = _automatedMessagesManager.GetAllMessages();
            AutomatedMessages.AddRange(msgs);
        }

        private void InitFilters()
        {
            Filters.Clear();
            var filters = _filterManager.GetAllFilters(FilterMode.All).Where(x => !x.IsWhitelist);
            Filters.AddRange(filters);
        }

        private void InitWhitelist()
        {
            Whitelist.Clear();
            var filters = _filterManager.GetAllFilters(FilterMode.Whitelist).Where(x => x.IsWhitelist);
            Whitelist.AddRange(filters);
        }

        private void InitUsers()
        {
            Users.Clear();
            var users = _usersManager.GetAllUsers();
            Users.AddRange(users);
        }

        private void InitCommands()
        {
            CustomCommands.Clear();
            var commands = _userCommandsManager.GetAllCommands();
            CustomCommands.AddRange(commands);
        }

        #region Filters

        [Command]
        public void FilterListDoubleClick()
        {
            if (SelectedFilter == null)
                return;
            if (!Prompt.Ask("Do you wish to delete " + SelectedFilter.Word + "?", "Delete"))
                return;

            _filterManager.RemoveFilterWord(SelectedFilter);
            InitWhitelist();
            InitFilters();
        }

        [Command]
        public void AddFilter()
        {
            _dialogService.ShowDialog<AddFilterDialog, AddFilterDialogViewModel>();
            InitWhitelist();
            InitFilters();
        }

        [Command]
        public void EditFilter()
        {
            if (SelectedFilter == null)
                return;

            _dialogService.ShowDialog<AddFilterDialog, AddFilterDialogViewModel>(SelectedFilter);
            InitWhitelist();
            InitFilters();
        }

        #endregion

        #region AutomatedMessages

        [Command]
        public void AutomatedMessagesDoubleClick()
        {
            if (SelectedAutomatedMessage == null)
                return;
            if (!Prompt.Ask("Do you wish to delete selected message?", "Delete"))
                return;

            _automatedMessagesManager.RemoveMessage(SelectedAutomatedMessage);
            InitAutomatedMessages();
        }

        [Command]
        public void AddAutomatedMessage()
        {
            _dialogService.ShowDialog<AddMessageDialog, AddMessageDialogViewModel>();
            InitAutomatedMessages();
        }

        [Command]
        public void EditAutomatedMessage()
        {
            if (SelectedAutomatedMessage == null)
                return;

            _dialogService.ShowDialog<AddMessageDialog, AddMessageDialogViewModel>(SelectedAutomatedMessage);
            InitAutomatedMessages();
        }

        #endregion

        #region Points
        [Command]
        public void UserDoubleClick()
        {
            if (SelectedUser == null)
                return;
            if (!Prompt.Ask("Do you wish to delete " + SelectedUser.VisibleName + "?", "Delete"))
                return;

            _usersManager.RemoveUser(SelectedUser);
            InitUsers();
        }

        [Command]
        public void AddUser()
        {
            _dialogService.ShowDialog<AddUserDialog, AddUserDialogViewModel>();
            InitUsers();
        }

        [Command]
        public void EditUser()
        {
            if (SelectedUser == null)
                return;

            _dialogService.ShowDialog<AddUserDialog, AddUserDialogViewModel>(SelectedUser);
            InitUsers();
        }

        #endregion
        #region CustomCommands

        [Command]
        public void CommandsDoubleClick()
        {
            if (SelectedCustomCommand == null)
                return;
            if (!Prompt.Ask("Do you wish to delete " + SelectedCustomCommand.Command + "?", "Delete"))
                return;

            _userCommandsManager.RemoveCommand(SelectedCustomCommand);
            InitCommands();
        }

        [Command]
        public void AddCustomCommand()
        {
            _dialogService.ShowDialog<AddCommandDialog, AddCommandDialogViewModel>();
            InitCommands();
        }

        [Command]
        public void EditCustomCommand()
        {
            if (SelectedCustomCommand == null)
                return;

            _dialogService.ShowDialog<AddCommandDialog, AddCommandDialogViewModel>(SelectedCustomCommand);
            InitCommands();
        }

        #endregion
    }
}
