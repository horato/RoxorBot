using System.Data.SQLite;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations.Database.Changescripts.Scripts
{
    [Changescript(1, 2, "Init database")]
    public class _1_InitDatabase : IChangescript
    {
        public void Execute(SQLiteConnection connection)
        {
            new SQLiteCommand("CREATE TABLE points (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, name Text NOT NULL, score INT NOT NULL);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE filters (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, word TEXT, banDuration int, author TEXT, isRegex BOOL DEFAULT 0, isWhitelist BOOL DEFAULT 0);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE messages (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, message TEXT, interval INT, 'enabled' BOOL DEFAULT 1);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE allowedUsers (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, name Text, allowed BOOL);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE userCommands (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp DATETIME, command TEXT, reply TEXT);", connection).ExecuteNonQuery();
        }
    }
}
