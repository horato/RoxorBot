using System.Data.SQLite;

namespace RoxorBot.Data.Interfaces
{
    public interface IChangescript
    {
        void Execute(SQLiteConnection connection);
    }
}
