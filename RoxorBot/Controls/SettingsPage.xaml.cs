using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoxorBot
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl
    {
        MainWindow mainWindow;
        public SettingsPage(MainWindow window)
        {
            InitializeComponent();
            SettingsContentControl.Content = new SettingsControl().Content;
            drawFilters();
            drawMessages();
            drawWhitelist();
            drawPoints();
            drawCommands();
            mainWindow = window;
            CloseButton.Click += mainWindow.CloseSettingsButton_OnClick;
        }

        ~SettingsPage()
        {
            mainWindow = null;
        }

        private void drawMessages()
        {
            AutomatedMessagesDataGrid.Items.Clear();
            var msgs = MessagesManager.getInstance().getAllMessages();
            foreach (AutomatedMessage item in msgs)
                AutomatedMessagesDataGrid.Items.Add(item);
        }

        private void drawFilters()
        {
            FilterListDataGrid.Items.Clear();
            var filters = FilterManager.getInstance().getAllFilters(FilterMode.All);
            foreach (FilterItem item in filters)
                if (!item.isWhitelist)
                    FilterListDataGrid.Items.Add(item);
        }

        private void drawWhitelist()
        {
            WhitelistDataGrid.Items.Clear();
            var filters = FilterManager.getInstance().getAllFilters(FilterMode.Whitelist);
            foreach (FilterItem item in filters)
                if (item.isWhitelist)
                    WhitelistDataGrid.Items.Add(item);
        }

        private void drawPoints()
        {
            PointsDataGrid.Items.Clear();
            var msgs = UsersManager.getInstance().getAllUsers();
            foreach (User item in msgs)
                if (item.Points > 0)
                    PointsDataGrid.Items.Add(item);
        }

        private void drawCommands()
        {
            CustomCommandsDataGrid.Items.Clear();
            var commands = UserCommandsManager.getInstance().getAllCommands();
            foreach (UserCommand command in commands)
                CustomCommandsDataGrid.Items.Add(command);
        }

        #region Filters

        private void FilterListDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var filterItem = dg.SelectedItem as FilterItem;
            if (filterItem == null)
                return;

            if (Prompt.Ask("Do you wish to delete " + filterItem.word + "?", "Delete"))
            {
                FilterManager.getInstance().removeFilterWord(filterItem.id);
                drawWhitelist();
                drawFilters();
            }
        }

        private void AddFilterButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddFilterDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.FilterWordBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.DurationBox.Text))
                    return;
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;

                var isWhitelist = (bool)dialog.IsWhitelistCheckBox.IsChecked.Value;
                int value;
                if (!int.TryParse(dialog.DurationBox.Text, out value))
                {
                    Logger.Log("Failed to int parse " + dialog.DurationBox.Text + " in  dialog.AddButton.Click");
                    return;
                }
                FilterManager.getInstance().addFilterWord(dialog.FilterWordBox.Text, value, "AdminConsole", (bool)dialog.IsRegexCheckBox.IsChecked, isWhitelist, dialog.id);
                drawWhitelist();
                drawFilters();
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            if (sender != null && sender is DataGrid)
            {
                var grid = sender as DataGrid;
                var selectedItem = grid.SelectedItem as FilterItem;
                if (selectedItem != null)
                {
                    dialog.id = selectedItem.id;
                    dialog.FilterWordBox.Text = selectedItem.word;
                    dialog.DurationBox.Text = selectedItem.duration.ToString();
                    dialog.IsRegexCheckBox.IsChecked = selectedItem.isRegex;
                    dialog.IsWhitelistCheckBox.IsChecked = selectedItem.isWhitelist;
                }
            }
            mainWindow.OverlayContainer.Content = dialog;
            mainWindow.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AddFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddFilterButton_Click(null, null);
        }

        private void EditFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddFilterButton_Click(FilterListDataGrid, null);
        }

        private void RemoveFilterMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FilterListDataGrid_OnMouseDoubleClick(FilterListDataGrid, null);
        }
        #endregion
        #region Whitelist
        private void EditWhitelistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddFilterButton_Click(WhitelistDataGrid, null);
        }

        private void RemoveWhitelistMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FilterListDataGrid_OnMouseDoubleClick(WhitelistDataGrid, null);
        }
        #endregion
        #region AutomatedMessages

        private void AutomatedMessageDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var msg = dg.SelectedItem as AutomatedMessage;
            if (msg == null)
                return;

            if (Prompt.Ask("Do you wish to delete selected message?", "Delete"))
            {
                MessagesManager.getInstance().removeMessage(msg);
                drawMessages();
            }
        }

        private void AddAutomatedMessage_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddMessageDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.MessageBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.IntervalBox.Text))
                    return;
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;

                int value;
                if (!int.TryParse(dialog.IntervalBox.Text, out value))
                {
                    Logger.Log("Failed to int parse " + dialog.IntervalBox.Text + " in MessagesManager.getInstance().addAutomatedMessage");
                    return;
                }
                MessagesManager.getInstance().addAutomatedMessage(dialog.MessageBox.Text, value, (mainWindow.c != null && mainWindow.c.IsConnected), dialog.active, dialog.id);

                drawMessages();
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            if (sender != null && sender is DataGrid)
            {
                var grid = sender as DataGrid;
                var msg = grid.SelectedItem as AutomatedMessage;
                if (msg != null)
                {
                    dialog.id = msg.id;
                    dialog.MessageBox.Text = msg.message;
                    dialog.IntervalBox.Text = msg.interval.ToString();
                    dialog.active = msg.active;
                }
            }
            mainWindow.OverlayContainer.Content = dialog;
            mainWindow.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AddMessageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddAutomatedMessage_Click(null, null);
        }

        private void EditMessageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddAutomatedMessage_Click(AutomatedMessagesDataGrid, null);
        }

        private void RemoveMessageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AutomatedMessageDataGrid_OnMouseDoubleClick(AutomatedMessagesDataGrid, null);
        }
        #endregion
        #region Points
        private void PointsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var filterItem = dg.SelectedItem as User;
            if (filterItem == null)
                return;

            if (Prompt.Ask("Do you wish to delete all of " + filterItem.Name + " points?", "Delete"))
            {
                PointsManager.getInstance().setPoints(filterItem.InternalName, 0);
                drawPoints();
            }

        }

        private void AddPointsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddPointsDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.NickTextBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.PointsTextBox.Text))
                    return;
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;

                int value;
                if (!int.TryParse(dialog.PointsTextBox.Text, out value))
                    return;

                if (value < 0)
                    value = 0;

                PointsManager.getInstance().setPoints(dialog.NickTextBox.Text, value);
                drawPoints();
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            if (sender != null && sender is DataGrid)
            {
                var grid = sender as DataGrid;
                var selectedItem = grid.SelectedItem as User;
                if (selectedItem != null)
                {
                    dialog.NickTextBox.Text = selectedItem.Name;
                    dialog.NickTextBox.IsEnabled = false;
                    dialog.PointsTextBox.Text = selectedItem.Points.ToString();
                }
            }
            mainWindow.OverlayContainer.Content = dialog;
            mainWindow.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void AddPointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddPointsButton_Click(null, null);
        }

        private void EditPointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddPointsButton_Click(PointsDataGrid, null);
        }

        private void RemovePointsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PointsDataGrid_OnMouseDoubleClick(PointsDataGrid, null);
        }
        #endregion
        #region CustomCommands

        private void CustomCommandsDataGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGrid))
                return;

            var dg = (DataGrid)sender;
            var filterItem = dg.SelectedItem as UserCommand;
            if (filterItem == null)
                return;

            if (Prompt.Ask("Do you wish to delete " + filterItem.command + "?", "Delete"))
            {
                UserCommandsManager.getInstance().removeCommand(filterItem.id);
                drawCommands();
            }
        }

        private void AddCommandMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddCommandButton_Click(null, null);
        }

        private void EditCommandMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AddCommandButton_Click(CustomCommandsDataGrid, null);
        }

        private void RemoveCommandMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CustomCommandsDataGrid_OnMouseDoubleClick(CustomCommandsDataGrid, null);
        }

        private void AddCommandButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsGrid.Opacity = 0.5;
            SettingsGrid.IsEnabled = false;
            var dialog = new AddCommandDialog();
            dialog.AddButton.Click += (a, b) =>
            {
                if (string.IsNullOrWhiteSpace(dialog.CommandBox.Text) ||
                    string.IsNullOrWhiteSpace(dialog.ReplyBox.Text))
                    return;

                UserCommandsManager.getInstance().addCommand(dialog.CommandBox.Text, dialog.ReplyBox.Text, dialog.id);
                drawCommands();

                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            dialog.CancelButton.Click += (a, b) =>
            {
                mainWindow.OverlayContainer.Visibility = Visibility.Hidden;
                SettingsGrid.Opacity = 1;
                SettingsGrid.IsEnabled = true;
            };
            if (sender != null && sender is DataGrid)
            {
                var grid = sender as DataGrid;
                var selectedItem = grid.SelectedItem as UserCommand;
                if (selectedItem != null)
                {
                    dialog.CommandBox.Text = selectedItem.command;
                    dialog.ReplyBox.Text = selectedItem.reply;
                    dialog.id = selectedItem.id;
                }
            }
            mainWindow.OverlayContainer.Content = dialog;
            mainWindow.OverlayContainer.Visibility = Visibility.Visible;
        }
        #endregion
    }
}
