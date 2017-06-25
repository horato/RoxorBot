using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RoxorBot.Data.Logic;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        //TODO: viewmodel
        public SettingsControl()
        {
            InitializeComponent();
            TimerRewardTextBox.Text = Properties.Settings.Default.timerReward.ToString();
            if (Properties.Settings.Default.twitch_login != null)
                TwitchLoginTextBox.Text = Properties.Settings.Default.twitch_login;
            MaxMessageLengthTextBox.Text = Properties.Settings.Default.maxMessageLength.ToString();
            sendTimeoutNotificationCheckBox.IsChecked = Properties.Settings.Default.notifyChatRestriction;
            MaxSongLengthTextBox.Text = Properties.Settings.Default.maxSongLength.ToString();
            notifyNextSongCheckBox.IsChecked = Properties.Settings.Default.notifyCurrentPlayingSong;

            renderButtons(TwitchPasswordContentControl);
        }

        private void renderButtons(ContentControl control)
        {
            var button = new Button();
            button.Content = "Show Password";
            button.Click += (a, b) =>
            {
                if (Prompt.Ask("Are you sure?", "Neukazuj to na strimu pyčo."))
                {
                    if (Properties.Settings.Default.twitch_oauth != null)
                        renderOauth();
                    else
                        TwitchPasswordContentControl.Content = "";
                }
            };

            TwitchPasswordContentControl.Content = button;
        }

        private void renderOauth()
        {
            var textbox = new TextBox();
            textbox.BorderThickness = new Thickness(0);
            textbox.Background = System.Windows.Media.Brushes.Transparent;
            textbox.Text = Properties.Settings.Default.twitch_oauth;
            textbox.KeyUp += textbox_KeyUp;
            TwitchPasswordContentControl.Content = textbox;
        }

        private void TimerRewardTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Char.IsNumber(Convert.ToChar(e.Text));
        }

        private void TimerRewardTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Space);
        }

        private void TimerRewardTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            int value;
            if (int.TryParse(TimerRewardTextBox.Text, out value))
                Properties.Settings.Default.timerReward = value;
            //else
            //    Logger.Log("Failed to int parse " + TimerRewardTextBox.Text + " in TimerRewardTextBox_KeyUp.");
        }

        void textbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox))
                return;

            Properties.Settings.Default.twitch_oauth = ((TextBox)sender).Text;
        }

        private void TwitchLogin_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox))
                return;

            Properties.Settings.Default.twitch_login = ((TextBox)sender).Text;
        }

        private void MaxMessageLengthTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox))
                return;

            var tb = (TextBox)sender;
            int value;

            if (!int.TryParse(tb.Text, out value))
            {
                //Logger.Log("Failed to int parse " + tb.Text + " in MaxMessageLengthTextBox_KeyUp");
                return;
            }
            Properties.Settings.Default.maxMessageLength = value;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox))
                return;
            var cb = (CheckBox)sender;
            Properties.Settings.Default.notifyChatRestriction = cb.IsChecked.Value;
        }

        private void NotifyNextSongCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is CheckBox))
                return;
            var cb = (CheckBox)sender;
            Properties.Settings.Default.notifyCurrentPlayingSong = cb.IsChecked.Value;
        }

        private void MaxSongLengthTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (!(sender is TextBox))
                return;

            var tb = (TextBox)sender;
            int value;

            if (!int.TryParse(tb.Text, out value))
            {
                //Logger.Log("Failed to int parse " + tb.Text + " in MaxSongLengthTextBox_KeyUp");
                return;
            }
            Properties.Settings.Default.maxSongLength = value;
        }
    }
}
