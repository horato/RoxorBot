using System;

namespace RoxorBot.Data.Model.Database.Entities
{
    public abstract class Entity
    {
        public virtual Guid Id { get; protected set; }
        public virtual byte[] DbTimestamp { get; }
    }
}
