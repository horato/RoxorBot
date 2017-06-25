using System;
using System.Collections.Generic;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Interfaces.Managers
{
    public interface IRaffleManager : IManagerBase
    {
        int PointsRequired { get; set; }
        bool IsFollowersOnly { get; set; }
        string RaffleName { get; set; }
        string AcceptedWords { get; }
        bool IsRunning { get; }
        void SetAcceptedWords(string words);
        void StartRaffle();
        void StopRaffle();
        void PickWinner();
        void OnUIClosing();
        IEnumerable<UserWrapper> GetAllParticipants();
        event EventHandler<UserWrapper> OnUserAdd;
        event EventHandler OnWinnerPicked;
    }
}
