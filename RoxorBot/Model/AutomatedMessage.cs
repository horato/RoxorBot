﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Model
{
    class AutomatedMessage
    {
        private bool _active = true;
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
                DatabaseManager.getInstance().executeNonQuery("UPDATE messages SET enabled=" + (_active ? "1" : "0") + " WHERE message='" + message + "';");
            }
        }
        public System.Timers.Timer timer { get; set; }
    }
}