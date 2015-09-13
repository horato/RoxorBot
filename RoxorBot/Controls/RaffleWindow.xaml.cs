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
using System.Windows.Threading;

namespace RoxorBot
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RaffleWindow : UserControl
    {
        private bool isRunning;

        public RaffleWindow()
        {
            InitializeComponent();
            isRunning = false;
            RaffleManager.getInstance().OnUserAdd += RaffleWindow_OnUserAdd;
            RaffleManager.getInstance().OnWinnerPicked += RaffleWindow_OnWinnerPicked;
        }

        private void FollowersOnlyCheckbox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox) || isRunning)
                return;

            var box = sender as CheckBox;
            RaffleManager.getInstance().setFollowersOnly(box.IsChecked.Value);
        }

        private void StartRaffleButton_Click(object sender, RoutedEventArgs e)
        {
            StartRaffleButton.IsEnabled = false;
            StopRaffleButton.IsEnabled = true;
            PointsRequiredTextBox.IsEnabled = false;
            FollowersOnlyCheckbox.IsEnabled = false;
            PickWinnerButton.IsEnabled = false;
            RaffleNameTextBox.IsEnabled = false;
            AcceptedWordsTextBox.IsEnabled = false;
            RaffleManager.getInstance().StartRaffle();
            isRunning = true;
        }

        private void StopRaffleButton_Click(object sender, RoutedEventArgs e)
        {
            StartRaffleButton.IsEnabled = true;
            StopRaffleButton.IsEnabled = false;
            PointsRequiredTextBox.IsEnabled = true;
            FollowersOnlyCheckbox.IsEnabled = true;
            PickWinnerButton.IsEnabled = true;
            RaffleNameTextBox.IsEnabled = true;
            AcceptedWordsTextBox.IsEnabled = true;
            RaffleManager.getInstance().StopRaffle();
            isRunning = false;
        }

        private void PickWinnerButton_Click(object sender, RoutedEventArgs e)
        {
            PickWinnerButton.IsEnabled = false;
            RaffleManager.getInstance().PickWinner();
        }
        private void PointsRequiredTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Char.IsNumber(Convert.ToChar(e.Text));
        }

        private void PointsRequiredTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Space);
        }

        private void PointsRequiredTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (isRunning)
                return;
            int value;

            if (!int.TryParse(PointsRequiredTextBox.Text, out value))
            {
                Logger.Log("Failed to int parse " + PointsRequiredTextBox.Text + " in PointsRequiredTextBox_KeyUp");
                return;
            }
            RaffleManager.getInstance().setPointsRequired(value);
        }

        private void RaffleNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isRunning)
                RaffleManager.getInstance().setRaffleName(RaffleNameTextBox.Text);
        }

        private void AcceptedWordsTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!isRunning)
                RaffleManager.getInstance().setAcceptedWords(AcceptedWordsTextBox.Text);
        }

        void RaffleWindow_OnUserAdd(object sender, User e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ParticipantsListView.Items.Add(e);
            }));
        }

        void RaffleWindow_OnWinnerPicked(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ParticipantsListView.Items.Clear();
            }));
        }
    }
}
