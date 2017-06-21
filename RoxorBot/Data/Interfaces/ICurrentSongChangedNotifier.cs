using System;

namespace RoxorBot.Data.Interfaces
{
    public interface ICurrentSongChangedNotifier
    {
        Action OnCurrentSongChanged { get; set; }
    }
}
