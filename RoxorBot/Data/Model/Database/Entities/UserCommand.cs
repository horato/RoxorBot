using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Model.Database.Entities
{
    public class UserCommand : Entity
    {
        public virtual string Command { get; set; }
        public virtual string Reply { get; set; }

        //nhibernate
        public UserCommand()
        {
        }

        public UserCommand(string command, string reply)
        {
            Command = command;
            Reply = reply;
        }
    }
}
