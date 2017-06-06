using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Enums;

namespace RoxorBot.Data.Interfaces
{
    public interface ILogger
    {
        void Log(string line, LogType type = LogType.Info);
    }
}
