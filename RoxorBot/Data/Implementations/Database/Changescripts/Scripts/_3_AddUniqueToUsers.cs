using System.Data.SQLite;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations.Database.Changescripts.Scripts
{
    [Changescript(3, 4, "Add unique constraint to Users table")]
    public class _3_AddUniqueToUsers : IChangescript
    {
        public void Execute(SQLiteConnection connection)
        {
            new SQLiteCommand("create unique index unique_name on users(ValueName);", connection).ExecuteNonQuery();
        }
    }
}
