using RoxorBot.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IEventAggregator _aggregator;
        private SQLiteConnection _dbConnection;

        public DatabaseManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing DatabaseManager...");
            InitDatabaseConnection();
        }

        private void InitDatabaseConnection()
        {
            if (!File.Exists("botDatabase.sqlite"))
            {
                SQLiteConnection.CreateFile("botDatabase.sqlite");
                _dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                _dbConnection.Open();
                new SQLiteCommand("CREATE TABLE points (name VARCHAR(64) PRIMARY KEY, score INT);", _dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE filters (id INTEGER PRIMARY KEY AUTOINCREMENT, word TEXT, duration TEXT, addedBy TEXT, isRegex BOOL DEFAULT 0, isWhitelist BOOL DEFAULT 0);", _dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE messages (id INTEGER PRIMARY KEY AUTOINCREMENT, message TEXT, interval INT, 'enabled' BOOL DEFAULT 1);", _dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE allowedUsers (name VARCHAR(64) PRIMARY KEY, allowed BOOL);", _dbConnection).ExecuteNonQuery();
                new SQLiteCommand("CREATE TABLE userCommands (id INTEGER PRIMARY KEY AUTOINCREMENT, command TEXT, reply TEXT);", _dbConnection).ExecuteNonQuery();
            }
            else
            {
                _dbConnection = new SQLiteConnection("Data Source=botDatabase.sqlite;Version=3;");
                _dbConnection.Open();
            }
        }

        public SQLiteDataReader ExecuteReader(string query)
        {
            return new SQLiteCommand(query, _dbConnection).ExecuteReader();
        }

        public int ExecuteNonQuery(string query)
        {
            return new SQLiteCommand(query, _dbConnection).ExecuteNonQuery();
        }

        public void Close()
        {
            _dbConnection.Close();
            _dbConnection.Dispose();
            _dbConnection = null;
        }
    }
}
