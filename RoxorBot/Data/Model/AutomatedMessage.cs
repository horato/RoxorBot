using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
{
    public class AutomatedMessage
    {
        private bool _active = true;
        public int id { get; set; }
        public string message { get; set; }
        public int interval { get; set; }

        [System.ComponentModel.DefaultValue(true)]
        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;
                //TODO:check
                //DatabaseManager.getInstance().ExecuteNonQuery("UPDATE messages SET enabled=" + (_active ? "1" : "0") + " WHERE message='" + message + "';");
            }
        }
        public System.Timers.Timer timer { get; set; }
    }
}
