using System;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;
using FluentNHibernate.Cfg;
using Microsoft.Practices.Unity;
using NHibernate.Cfg;
using Prism.Events;
using RoxorBot.Data;
using RoxorBot.Data.Events;
using RoxorBot.Data.Implementations.Database.Changescripts;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Logic.Managers
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly IEventAggregator _aggregator;

        public DatabaseManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
        }

        private void InitDatabaseConnection()
        {
            var xmlConfig = new NHibernate.Cfg.Configuration();
            xmlConfig.Configure();

            var sessionFactory = Fluently.Configure(xmlConfig)
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Bootstrapper>().Conventions.AddFromAssemblyOf<Bootstrapper>())
                .ExposeConfiguration(ConfigureDatabaseIfNeeded)
                .BuildSessionFactory();

            DI.Container.RegisterInstance(sessionFactory, new ContainerControlledLifetimeManager());
        }

        private void ConfigureDatabaseIfNeeded(Configuration obj)
        {
            string connectionString;
            if (!obj.Properties.TryGetValue("connection.connection_string", out connectionString))
                throw new InvalidOperationException("Connection string not found.");

            CreateTableIfNeeded(connectionString);

            var dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();
            ChangescriptsHandler.ApplyChangescripts(dbConnection);
            dbConnection.Close();
            dbConnection.Dispose();
        }

        private void CreateTableIfNeeded(string connectionString)
        {
            var regex = Regex.Match(connectionString, "Data Source=(.*?)(;|$)");
            if (!regex.Success)
                throw new InvalidOperationException($"Invalid connection string {connectionString}");

            var dbFileName = regex.Groups[1].Value;
            if (File.Exists(dbFileName))
                return;

            SQLiteConnection.CreateFile(dbFileName);
            var dbConnection = new SQLiteConnection(connectionString);
            dbConnection.Open();

            var createChangescriptTable = @"
CREATE TABLE Changescripts(
	Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	CurrentVersion INTEGER NOT NULL,
	PreviousVersion INTEGER NOT NULL,
	ChangescriptDescription Text NULL,
	CreatedOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO Changescripts (CurrentVersion,PreviousVersion,ChangescriptDescription) SELECT '1', '0', 'Initialize';
";
            new SQLiteCommand(createChangescriptTable, dbConnection).ExecuteNonQuery();
            dbConnection.Close();
            dbConnection.Dispose();
        }

        public void Init()
        {
            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing DatabaseManager...");
            InitDatabaseConnection();
        }
    }
}
