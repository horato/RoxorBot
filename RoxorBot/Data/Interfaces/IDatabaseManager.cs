using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Interfaces
{
    public interface IDatabaseManager
    {
        SQLiteDataReader ExecuteReader(string query);
        int ExecuteNonQuery(string query);
        void Close();
    }
}
