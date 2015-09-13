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
    /// Interaction logic for SettingsControl.xaml
    /// </summary>
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
            TimerRewardTextBox.Text = Properties.Settings.Default.timerReward.ToString();
            if (Properties.Settings.Default.twitch_login != null)
                TwitchLoginTextBox.Text = Properties.Settings.Default.twitch_login;
            MaxMessageLengthTextBox.Text = Properties.Settings.Default.maxMessageLength.ToString();

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
            else
                Logger.Log("Failed to int parse " + TimerRewardTextBox.Text + " in TimerRewardTextBox_KeyUp.");
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
                Logger.Log("Failed to int parse " + tb.Text + " in MaxMessageLengthTextBox_KeyUp");
                return;
            }
            Properties.Settings.Default.maxMessageLength = value;
        }
    }
}
