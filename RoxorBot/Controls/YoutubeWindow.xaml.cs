using Awesomium.Core;
using RoxorBot.Model.Youtube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
    #region secret stash
    // if callbacks are needed in future
    // JSObject jsobject = browser.CreateGlobalJavascriptObject("VideoCallback");
    // jsobject.Bind("getNextSongUrl", sasd);
    // browser.ExecuteJavascript("function playSong(url) { player.src = url; player.load(); player.play(); }");
    // browser.ExecuteJavascript("playSong('" + YoutubeManager.getInstance().getVideoDirectLink("RSdKmX2BH7o") + "');");
    /*private JSValue sasd(JSValue[] arguments)
    {
        foreach (var a in arguments)
            System.Diagnostics.Debug.Write(a + " ");
        System.Diagnostics.Debug.WriteLine("");
        return "";
    }*/
    #endregion
    /// <summary>
    /// Interaction logic for PlugDJWindow.xaml
    /// </summary>
    public partial class YoutubeWindow : Window
    {
        public bool close = false;
        private dynamic videoPlayer;
        private System.Timers.Timer playTimer;
        private System.Timers.Timer updateTimer;
        private YoutubeVideo currentVideo;

        public YoutubeWindow()
        {
            InitializeComponent();
#if DEBUG
            if (!WebCore.IsInitialized)
                WebCore.Initialize(new WebConfig()
                {
                    RemoteDebuggingHost = "127.0.0.1",
                    RemoteDebuggingPort = 2229,
                    LogLevel = LogLevel.Verbose
                });
            Button.Visibility = Visibility.Visible;
#endif
            MainWindow.ChatMessageReceived += MainWindow_ChatMessageReceived;
            browser.Loaded += browser_Loaded;
            playTimer = new System.Timers.Timer(5000);
            playTimer.AutoReset = true;
            playTimer.Elapsed += playTimer_Elapsed;
            updateTimer = new System.Timers.Timer(500);
            updateTimer.AutoReset = true;
            updateTimer.Elapsed += (a, b) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    PrimaryQueueCount.Content = YoutubeManager.getInstance().getPlaylistCount();
                    SecondaryQueueCount.Content = YoutubeManager.getInstance().getBackupPlaylistCount();
                    if (videoPlayer != null && videoPlayer.duration != double.NaN && videoPlayer.currentTime != double.NaN)
                    {
                        PlayProgressSlider.Maximum = (double)videoPlayer.duration;
                        PlayProgressSlider.Value = (double)videoPlayer.currentTime;
                    }
                }));
            };
            updateTimer.Start();
        }

        void playTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (isPlaying())
                        return;

                    getNextAndPlay();
                }));
        }

        void MainWindow_ChatMessageReceived(object sender, IrcDotNet.IrcRawMessageEventArgs e)
        {
            if (!(sender is MainWindow))
                return;
            if (e.Message.Parameters.Count < 2)
                return;

            var mainWindow = (MainWindow)sender;
            var msg = e.Message.Parameters[1];

            if (msg.StartsWith("!songrequest "))
            {
                var commands = msg.Split(' ');
                if (commands.Length < 2)
                    return;
                var link = commands[1];
                var match = Regex.Match(link, "youtu(?:\\.be|be\\.com)\\/(?:.*v(?:\\/|=)|(?:.*\\/)?)([a-zA-Z0-9-_]+)");
                if (match.Success)
                {
                    try
                    {
                        var id = match.Groups[1].Value;
                        var video = YoutubeManager.getInstance().addSong(id);
                        if (video != null)
                        {
                            if (video.duration.TotalSeconds > Properties.Settings.Default.maxSongLength)
                            {
                                YoutubeManager.getInstance().removeSong(video.id);
                                throw new VideoParseException("Video " + id + " is too long. Max length is " + Properties.Settings.Default.maxSongLength + "s.");
                            }
                            video.requester = e.Message.Source.Name;
                            mainWindow.sendChatMessage(e.Message.Source.Name + ": " + video.info.snippet.title + " added to queue.");
                        }
                    }
                    catch (VideoParseException ee)
                    {
                        Logger.Log(ee.Message);
                        mainWindow.sendChatMessage(e.Message.Source.Name + ": " + ee.Message);
                    }
                    catch (Exception) { }
                }
            }
            else if (msg.Equals("!skipsong") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    getNextAndPlay();
                }));
            }
            else if (msg.StartsWith("!volume ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                var commands = msg.Split(' ');
                if (commands.Length < 2)
                    return;

                int volume;

                if (int.TryParse(commands[1], out volume))
                {
                    if (volume < 1 || volume > 100)
                        return;

                    setVolume(volume / 100.0);
                    mainWindow.sendChatMessage(e.Message.Source.Name + ": Volume set to " + volume);
                }
            }
        }

        private void setVolume(double volume)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                videoPlayer.volume = volume;
                VolumeSlider.Value = volume;
            }));
        }

        private void getNextAndPlay()
        {
            playTimer.Stop();
            currentVideo = YoutubeManager.getInstance().getNextAndRemove();
            videoPlayer.src = currentVideo.embedLink;
            videoPlayer.load();
            playTimer.Start();
            CurrentlyPlayingLabel.Text = currentVideo.name + (string.IsNullOrWhiteSpace(currentVideo.requester) ? "" : " --- Requested by: " + currentVideo.requester);
        }

        void browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.DocumentReady += browser_DocumentReady;
            browser.Source = new Uri(YoutubeManager.getVideoDirectLink());
        }

        void browser_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            browser.DocumentReady -= browser_DocumentReady;

            browser.ExecuteJavascript("var player = document.getElementsByTagName(\"video\")[0];");
            browser.ExecuteJavascript("player.volume=0.2;");
            browser.ExecuteJavascript("player.controls=false;");
            browser.ExecuteJavascript("player.pause();");
            videoPlayer = (JSObject)browser.ExecuteJavascriptWithResult("player");

            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                PauseButton.IsEnabled = true;
                StartButton.IsEnabled = true;
                VolumeSlider.Value = 0.2;
            }));
        }

        private bool isPlaying()
        {
            var x = (double)videoPlayer.currentTime;
            var y = (bool)videoPlayer.paused;
            var z = (bool)videoPlayer.ended;
            return x > 0 && y == false && z == false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (close)
            {
                videoPlayer.pause();
                videoPlayer.currentTime = 0;
                playTimer.Stop();
                updateTimer.Stop();
            }
            else
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.src = null;
            videoPlayer.load();
        }

        private void StartDJButton_Click(object sender, RoutedEventArgs e)
        {
            getNextAndPlay();
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
            PauseButton.IsEnabled = true;
            PauseButton.Content = "Pause";
        }

        private void StopDJButton_Click(object sender, RoutedEventArgs e)
        {
            playTimer.Stop();
            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            SkipButton.IsEnabled = false;
            PauseButton.IsEnabled = false;
        }

        private void SkipSongButton_Click(object sender, RoutedEventArgs e)
        {
            videoPlayer.pause();
            getNextAndPlay();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (PauseButton.Content.ToString() == "Pause")
            {
                playTimer.Stop();
                videoPlayer.pause();
                PauseButton.Content = "Play";
            }
            else
            {
                if (StopButton.IsEnabled)
                    playTimer.Start();
                videoPlayer.play();
                PauseButton.Content = "Pause";
            }
        }


        private void PlayProgressSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (!(sender is Slider))
                return;
            var slider = (Slider)sender;

            videoPlayer.currentTime = slider.Value;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!(sender is Slider))
                return;
            var slider = (Slider)sender;

            setVolume(slider.Value);
        }
    }
}
