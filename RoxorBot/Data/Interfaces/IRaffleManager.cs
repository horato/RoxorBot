using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Model;

namespace RoxorBot.Data.Interfaces
{
    public interface IRaffleManager
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
        IEnumerable<User> GetAllParticipants();
        event EventHandler<User> OnUserAdd;
        event EventHandler OnWinnerPicked;
    }
}
