using System;

namespace RoxorBot.Data.Model.Database.Entities
{
    public class AutomatedMessage : Entity
    {
        public virtual string Message { get; set; }
        public virtual int Interval { get; set; }
        public virtual bool Enabled { get; set; }

        //nhibernate
        public AutomatedMessage()
        {

        }

        public AutomatedMessage(string message, int interval, bool enabled)
        {
            Message = message;
            Interval = interval;
            Enabled = enabled;
        }

        public AutomatedMessage(Guid id, string message, int interval, bool enabled) : this(message, interval, enabled)
        {
            Id = id;
        }
    }
}
