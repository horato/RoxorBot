using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Interfaces
{
    public interface ICurrentSongChangedNotifier
    {
        Action OnCurrentSongChanged { get; set; }
    }
}
