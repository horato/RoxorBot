using System;
using System.Collections.ObjectModel;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Managers;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class RaffleViewModel : BindableBase, IDisposable
    {
        private readonly IRaffleManager _raffleManager;
        private bool _canPickWinner;

        public bool IsFollowersOnly
        {
            get { return _raffleManager.IsFollowersOnly; }
            set
            {
                _raffleManager.IsFollowersOnly = value;
                RaisePropertyChanged();
            }
        }

        public int RequiredPoints
        {
            get { return _raffleManager.PointsRequired; }
            set
            {
                _raffleManager.PointsRequired = value;
                RaisePropertyChanged();
            }
        }

        public string RaffleName
        {
            get { return _raffleManager.RaffleName; }
            set
            {
                _raffleManager.RaffleName = value;
                RaisePropertyChanged();
            }
        }

        public string AcceptedWords
        {
            get { return _raffleManager.AcceptedWords; }
            set
            {
                _raffleManager.SetAcceptedWords(value);
                RaisePropertyChanged();
            }
        }

        public bool IsRunning => _raffleManager.IsRunning;
        public bool IsNotRunning => !IsRunning;
        public ObservableCollection<UserWrapper> Participants { get; } = new ObservableCollection<UserWrapper>();

        public RaffleViewModel(IRaffleManager raffleManager)
        {
            _raffleManager = raffleManager;
            Participants.Clear();
            Participants.AddRange(_raffleManager.GetAllParticipants());
            _raffleManager.OnUserAdd += RaffleManagerOnOnUserAdd;
            _raffleManager.OnWinnerPicked += RaffleManagerOnOnWinnerPicked;
        }

        [Command]
        public void StartRaffle()
        {
            _raffleManager.StartRaffle();
            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));
            _canPickWinner = false;
        }

        public bool CanStartRaffle()
        {
            return !_raffleManager.IsRunning;
        }

        [Command]
        public void StopRaffle()
        {
            _raffleManager.StopRaffle();
            RaisePropertyChanged(nameof(IsRunning));
            RaisePropertyChanged(nameof(IsNotRunning));
            _canPickWinner = true;
        }

        public bool CanStopRaffle()
        {
            return _raffleManager.IsRunning;
        }

        [Command]
        public void PickWinner()
        {
            _canPickWinner = false;
            _raffleManager.PickWinner();
        }

        public bool CanPickWinner()
        {
            return _canPickWinner;
        }

        public void Dispose()
        {
            _raffleManager.OnUserAdd -= RaffleManagerOnOnUserAdd;
            _raffleManager.OnWinnerPicked -= RaffleManagerOnOnWinnerPicked;
        }

        private void RaffleManagerOnOnUserAdd(object sender, UserWrapper user)
        {
            Participants.Add(user);
        }

        private void RaffleManagerOnOnWinnerPicked(object sender, EventArgs eventArgs)
        {
            Participants.Clear();
        }

        [Command]
        public void Close()
        {
            _raffleManager.OnUIClosing();
        }
    }
}
