using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Model.Database.Entities
{
    public abstract class Entity
    {
        public virtual Guid Id { get; protected set; }
        public virtual byte[] DbTimestamp { get; }
    }
}
