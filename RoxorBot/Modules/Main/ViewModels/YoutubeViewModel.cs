using System;
using System.IO;
using System.Windows.Threading;
using Prism.Events;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Events;
using RoxorBot.Data.Events.Youtube;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Chat;
using RoxorBot.Data.Model.Youtube;
using Vlc.DotNet.Core;
using Vlc.DotNet.Forms;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class YoutubeViewModel : BindableBase, ICurrentSongChangedNotifier
    {
        private readonly IChatManager _chatManager;
        private readonly ILogger _logger;
        private readonly IYoutubeManager _youtubeManager;
        private readonly IUsersManager _usersManager;
        private readonly IEventAggregator _aggregator;
        private int _volume;
        private bool _isPaused;
        private YoutubeVideo _currentVideo;

        public VlcControl Player { get; set; } = new VlcControl();
        public int PrimaryQueueCount => _youtubeManager.PlaylistCount;
        public int SecondaryQueueCount => _youtubeManager.BackupPlaylistCount;
        public float SeekSliderValue { get { return Player.Position; } set { Player.Position = value; RaisePropertyChanged(); } }
        public string CurrentSongText => _currentVideo?.Name + (string.IsNullOrWhiteSpace(_currentVideo?.Requester) ? "" : " --- Requested by: " + _currentVideo?.Requester);
        public Action OnCurrentSongChanged { get; set; }
        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                if (Player.Audio != null)
                    Player.Audio.Volume = value;

                RaisePropertyChanged();
            }
        }


        public YoutubeViewModel(IChatManager chatManager, ILogger logger, IYoutubeManager youtubeManager, IUsersManager usersManager, IEventAggregator aggregator)
        {
            _chatManager = chatManager;
            _logger = logger;
            _youtubeManager = youtubeManager;
            _usersManager = usersManager;
            _aggregator = aggregator;

            _aggregator.GetEvent<GetCurrentSongEvent>().Subscribe(OnGetCurrentSong);
            _aggregator.GetEvent<GetSetVolumeEvent>().Subscribe(OnGetSetVolume);
            _aggregator.GetEvent<SkipCurrentSongEvent>().Subscribe(OnSkipCurrentSong);
            _aggregator.GetEvent<VideoAddedEvent>().Subscribe(OnVideoAdded);

            Player.PositionChanged += PlayerPositionChanged;
            Player.EndReached += PlayerEndReached;
            Player.PositionChanged += PlayerPositionChanged;

            Player.Play(_youtubeManager.GetVideoDirectLink());
            Player.Pause();
            Volume = 50;
        }

        private void OnGetCurrentSong(GetCurrentSongEventArgs obj)
        {
            obj.Video = _currentVideo;
        }

        private void OnGetSetVolume(GetSetVolumeEventArgs obj)
        {
            if (obj.NewVolume > 0)
                Volume = obj.NewVolume;

            obj.CurrentVolume = Volume;
        }

        private void OnVideoAdded(VideoAddedEventArgs obj)
        {
            RaisePropertyChanged(nameof(PrimaryQueueCount));
            RaisePropertyChanged(nameof(SecondaryQueueCount));
        }

        private void OnSkipCurrentSong()
        {
            GetNextAndPlay();
        }

        private void PlayerEndReached(object sender, VlcMediaPlayerEndReachedEventArgs e)
        {
            GetNextAndPlay();
        }

        private void PlayerPositionChanged(object sender, VlcMediaPlayerPositionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(SeekSliderValue));
        }

        [Command]
        public void Start()
        {
            if (_isPaused)
                Player.Play();
            else
                GetNextAndPlay();

            _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
        }

        public bool CanStart()
        {
            return true;
        }

        [Command]
        public void Stop()
        {
            Player.Stop();
            _isPaused = false;
            _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
        }

        public bool CanStop()
        {
            return true;
        }

        [Command]
        public void SkipSong()
        {
            GetNextAndPlay();
        }

        [Command]
        public void Pause()
        {
            Player.Pause();
            _isPaused = true;
            _aggregator.GetEvent<RaiseButtonsEnabled>().Publish();
        }

        public bool CanPause()
        {
            return true;
        }

        private void GetNextAndPlay()
        {
            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                Player.Stop();
                _currentVideo = _youtubeManager.GetNextAndRemove();
                Player.Play(_currentVideo.EmbedLink);
                RaisePropertyChanged(nameof(CurrentSongText));
                OnCurrentSongChanged?.Invoke();

                try
                {
                    File.WriteAllText("playing.txt", _currentVideo.Name);
                }
                catch { }

                if (Properties.Settings.Default.notifyCurrentPlayingSong)
                    _chatManager.SendChatMessage("Next song: " + _currentVideo.Name);
            }));
        }
    }
}
