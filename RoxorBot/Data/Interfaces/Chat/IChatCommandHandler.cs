using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Models.Client;

namespace RoxorBot.Data.Interfaces.Chat
{
    public interface IChatCommandHandler
    {
        bool CanHandle(string command);
        void Handle(ChatCommand command);
    }
}
