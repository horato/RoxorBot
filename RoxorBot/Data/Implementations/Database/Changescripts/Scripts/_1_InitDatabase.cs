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
            new SQLiteCommand("CREATE TABLE points (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp Timestamp DEFAULT CURRENT_TIMESTAMP, name Text NOT NULL, score INT NOT NULL);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE filters (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp Timestamp DEFAULT CURRENT_TIMESTAMP, word TEXT NOT NULL, banDuration int, author TEXT NOT NULL, isRegex BOOL DEFAULT 0, isWhitelist BOOL DEFAULT 0);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE messages (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp Timestamp DEFAULT CURRENT_TIMESTAMP, message TEXT NOT NULL, interval INT, 'enabled' BOOL DEFAULT 1);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE allowedUsers (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp Timestamp DEFAULT CURRENT_TIMESTAMP, name Text, allowed BOOL);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE userCommands (Id UNIQUEIDENTIFIER PRIMARY KEY, DbTimestamp Timestamp DEFAULT CURRENT_TIMESTAMP, command TEXT NOT NULL, reply TEXT NOT NULL);", connection).ExecuteNonQuery();
        }
    }
}
