using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RoxorBot.Data.Interfaces;
using Vlc.DotNet.Forms;

namespace RoxorBot.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for PlugDJWindow.xaml
    /// </summary>
    public partial class YoutubeView : Window
    {
        public bool close = true;

        public YoutubeView()
        {
            DataContextChanged += YoutubeViewDataContextChanged;
            InitializeComponent();

            var player = Player.Player;
            player.VlcLibDirectoryNeeded += MediaPlayer_VlcLibDirectoryNeeded;
            player.EndInit();
            InitMarquee();
        }

        private void YoutubeViewDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var notifier = DataContext as ICurrentSongChangedNotifier;
            if (notifier == null)
                return;

            notifier.OnCurrentSongChanged = InitMarquee;
        }

        private void MediaPlayer_VlcLibDirectoryNeeded(object sender, VlcLibDirectoryNeededEventArgs e)
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
                return;
            if (AssemblyName.GetAssemblyName(currentAssembly.Location).ProcessorArchitecture == ProcessorArchitecture.X86)
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"Lib\x86\"));
            else
                e.VlcLibDirectory = new DirectoryInfo(Path.Combine(currentDirectory, @"Lib\x64\"));
        }

        private void InitMarquee()
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                if (CurrentlyPlayingLabel.ActualWidth > canMain.Width)
                    RightToLeftMarquee();
                else
                    CurrentlyPlayingLabel.BeginAnimation(Canvas.RightProperty, null);
            }));

        }

        private void RightToLeftMarquee()
        {
            double height = canMain.ActualHeight - CurrentlyPlayingLabel.ActualHeight;
            CurrentlyPlayingLabel.Margin = new Thickness(0, height / 2, 0, 0);
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = -CurrentlyPlayingLabel.ActualWidth;
            doubleAnimation.To = canMain.ActualWidth;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(20));
            CurrentlyPlayingLabel.BeginAnimation(Canvas.RightProperty, doubleAnimation);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (close)
            {
                //videoPlayer.pause();
                //videoPlayer.currentTime = 0;
                Player.Player.Stop();
                Player.Player.Dispose();
                Player.Dispose();
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }
    }
}
