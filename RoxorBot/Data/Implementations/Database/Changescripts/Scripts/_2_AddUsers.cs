using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations.Database.Changescripts.Scripts
{
    [Changescript(2, 3, "Add Users table and remove points & allowed users")]
    public class _2_AddUsers : IChangescript
    {
        public void Execute(SQLiteConnection connection)
        {
            new SQLiteCommand("DROP TABLE points;", connection).ExecuteNonQuery();
            new SQLiteCommand("DROP TABLE allowedUsers;", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE users (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, VisibleName TEXT, ValueName TEXT, Role TEXT, IsOnline BOOL, Points INT, IsFollower BOOL, IsFollowerSince DATETIME, IsAllowed BOOL);", connection).ExecuteNonQuery();
        }
    }
}
