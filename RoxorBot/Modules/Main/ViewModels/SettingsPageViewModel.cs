using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Implementations;
using RoxorBot.Data.Interfaces;
using RoxorBot.Model;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class SettingsPageViewModel
    {
        private readonly IAutomatedMessagesManager _automatedMessagesManager;
        private readonly IChatManager _chatManager;
        private readonly ILogger _logger;
        private readonly IFilterManager _filterManager;
        private readonly IUsersManager _usersManager;
        private readonly IUserCommandsManager _userCommandsManager;
        private readonly IPointsManager _pointsManager;

        public ObservableCollection<AutomatedMessage> AutomatedMessages { get; } = new ObservableCollection<AutomatedMessage>();
        public ObservableCollection<FilterItem> Filters { get; } = new ObservableCollection<FilterItem>();
        public ObservableCollection<FilterItem> Whitelist { get; } = new ObservableCollection<FilterItem>();
        public ObservableCollection<User> Points { get; } = new ObservableCollection<User>();
        public ObservableCollection<UserCommand> CustomCommands { get; } = new ObservableCollection<UserCommand>();

        public AutomatedMessage SelectedAutomatedMessage { get; set; }
        public FilterItem SelectedFilter { get; set; }
        public User SelectedPointsRow { get; set; }
        public UserCommand SelectedCustomCommand { get; set; }

        public SettingsPageViewModel(IAutomatedMessagesManager automatedMessagesManager, IChatManager chatManager, ILogger logger, IFilterManager filterManager, IUsersManager usersManager, IUserCommandsManager userCommandsManager, IPointsManager pointsManager)
        {
            _automatedMessagesManager = automatedMessagesManager;
            _chatManager = chatManager;
            _logger = logger;
            _filterManager = filterManager;
            _usersManager = usersManager;
            _userCommandsManager = userCommandsManager;
            _pointsManager = pointsManager;

            InitFilters();
            InitAutomatedMessages();
            InitWhitelist();
            InitPoints();
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
            var filters = _filterManager.GetAllFilters(FilterMode.All).Where(x => !x.isWhitelist);
            Filters.AddRange(filters);
        }

        private void InitWhitelist()
        {
            Whitelist.Clear();
            var filters = _filterManager.GetAllFilters(FilterMode.Whitelist).Where(x => x.isWhitelist);
            Whitelist.AddRange(filters);
        }

        private void InitPoints()
        {
            Points.Clear();
            var users = _usersManager.GetAllUsers().Where(x => x.Points > 0);
            Points.AddRange(users);
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
            if (!Prompt.Ask("Do you wish to delete " + SelectedFilter.word + "?", "Delete"))
                return;

            _filterManager.RemoveFilterWord(SelectedFilter.id);
            InitWhitelist();
            InitFilters();
        }

        [Command]
        public void AddFilter()
        {
            //TODO: viewmodel
            var dialog = new AddFilterDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.FilterWordBox.Text) || string.IsNullOrWhiteSpace(dialog.DurationBox.Text))
                    return;

                var isWhitelist = (bool)dialog.IsWhitelistCheckBox.IsChecked.Value;
                int value;
                if (!int.TryParse(dialog.DurationBox.Text, out value))
                {
                    _logger.Log("Failed to int parse " + dialog.DurationBox.Text + " in  dialog.AddButton.Click");
                    return;
                }
                _filterManager.AddFilterWord(dialog.FilterWordBox.Text, value, "AdminConsole", (bool)dialog.IsRegexCheckBox.IsChecked, isWhitelist, dialog.id);
                InitWhitelist();
                InitFilters();
                dialog.Close();
            };

            if (SelectedFilter != null)
            {
                dialog.id = SelectedFilter.id;
                dialog.FilterWordBox.Text = SelectedFilter.word;
                dialog.DurationBox.Text = SelectedFilter.duration.ToString();
                dialog.IsRegexCheckBox.IsChecked = SelectedFilter.isRegex;
                dialog.IsWhitelistCheckBox.IsChecked = SelectedFilter.isWhitelist;
            }
            dialog.ShowDialog();
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
            //TODO: viewmodel
            var dialog = new AddMessageDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.MessageBox.Text) || string.IsNullOrWhiteSpace(dialog.IntervalBox.Text))
                    return;

                int value;
                if (!int.TryParse(dialog.IntervalBox.Text, out value))
                {
                    _logger.Log("Failed to int parse " + dialog.IntervalBox.Text + " in MessagesManager.getInstance().addAutomatedMessage");
                    return;
                }

                _automatedMessagesManager.AddAutomatedMessage(dialog.MessageBox.Text, value, _chatManager.IsConnected, dialog.active, dialog.id);
                InitAutomatedMessages();
                dialog.Close();
            };

            if (SelectedAutomatedMessage != null)
            {
                dialog.id = SelectedAutomatedMessage.id;
                dialog.MessageBox.Text = SelectedAutomatedMessage.message;
                dialog.IntervalBox.Text = SelectedAutomatedMessage.interval.ToString();
                dialog.active = SelectedAutomatedMessage.active;

            }
            dialog.ShowDialog();
        }
        #endregion

        #region Points
        [Command]
        public void PointsDoubleClickCommand()
        {
            if (SelectedPointsRow == null)
                return;
            if (!Prompt.Ask("Do you wish to delete all of " + SelectedPointsRow.Name + " points?", "Delete"))
                return;

            _pointsManager.SetPoints(SelectedPointsRow.InternalName, 0);
            InitPoints();
        }

        [Command]
        public void AddPoints()
        {
            //TODO: viewmodel
            var dialog = new AddPointsDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.NickTextBox.Text) || string.IsNullOrWhiteSpace(dialog.PointsTextBox.Text))
                    return;

                int value;
                if (!int.TryParse(dialog.PointsTextBox.Text, out value))
                    return;

                if (value < 0)
                    value = 0;

                _pointsManager.SetPoints(dialog.NickTextBox.Text, value);
                InitPoints();
                dialog.Close();
            };

            if (SelectedPointsRow != null)
            {
                dialog.NickTextBox.Text = SelectedPointsRow.Name;
                dialog.NickTextBox.IsEnabled = false;
                dialog.PointsTextBox.Text = SelectedPointsRow.Points.ToString();
            }

            dialog.ShowDialog();
        }

        #endregion
        #region CustomCommands

        [Command]
        public void CommandsDoubleClick()
        {
            if (SelectedCustomCommand == null)
                return;
            if (!Prompt.Ask("Do you wish to delete " + SelectedCustomCommand.command + "?", "Delete"))
                return;

            _userCommandsManager.RemoveCommand(SelectedCustomCommand.id);
            InitCommands();
        }

        [Command]
        public void AddCustomCommand()
        {
            var dialog = new AddCommandDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.CommandBox.Text) || string.IsNullOrWhiteSpace(dialog.ReplyBox.Text))
                    return;

                _userCommandsManager.AddCommand(dialog.CommandBox.Text, dialog.ReplyBox.Text, dialog.id);
                InitCommands();
                dialog.Close();
            };

            if (SelectedCustomCommand != null)
            {
                dialog.CommandBox.Text = SelectedCustomCommand.command;
                dialog.ReplyBox.Text = SelectedCustomCommand.reply;
                dialog.id = SelectedCustomCommand.id;
            }

            dialog.ShowDialog();
        }
        #endregion
    }
}
