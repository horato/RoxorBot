using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class DatabaseManager
    {
        private static DatabaseManager _instance;
        private SQLiteConnection dbConnection;

        private DatabaseManager()
        {
            Logger.Log("Initializing DatabaseManager...");
            initDatabaseConnection();
        }

        private void initDatabaseConnection()
        {
            if (!File.Exists("botDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("botDatabase.sqlite");
                dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                dbConnection.Open();
                new SQLiteCommand("CREATE TABLE points (name VARCHAR(64) PRIMARY KEY, score INT);", dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE filters (word TEXT PRIMARY KEY, duration TEXT, addedBy TEXT, isRegex BOOL DEFAULT 0, isWhitelist BOOL DEFAULT 0);", dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE messages (message TEXT PRIMARY KEY, interval INT, 'enabled' BOOL DEFAULT 1);", dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE allowedUsers (name VARCHAR(64) PRIMARY KEY, allowed BOOL);", dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE userCommands (id INT PRIMARY KEY AUTOINCREMENT, command TEXT, reply TEXT);", dbConnection).ExecuteNonQuery();
            }
            else
            {
                dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                dbConnection.Open();
            }
        }

        public SQLiteDataReader executeReader(string query)
        {
            return new SQLiteCommand(query, dbConnection).ExecuteReader();
        }

        public int executeNonQuery(string query)
        {
            return new SQLiteCommand(query, dbConnection).ExecuteNonQuery();
        }

        public void close()
        {
            dbConnection.Close();
            dbConnection = null;
            _instance = null;
        }

        public static DatabaseManager getInstance()
        {
            if (_instance == null)
                _instance = new DatabaseManager();

            return _instance;
        }
    }
}
