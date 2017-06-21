using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;
using RoxorBot.Data.Implementations.Database;

namespace RoxorBot.Data.Interfaces
{
    public interface IDatabaseManager
    {
        void Init();
    }
}
