using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentNHibernate.Cfg;
using NHibernate;
using NHibernate.Cfg;
using RoxorBot.Data.Implementations.Database.Changescripts;

namespace RoxorBot.Data.Implementations.Database
{
    public static class DatabaseConfigurator
    {
        private const string CreateChangescriptTableQuery = @"
CREATE TABLE Changescripts(
	Id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	CurrentVersion INTEGER NOT NULL,
	PreviousVersion INTEGER NOT NULL,
	ChangescriptDescription Text NULL,
	CreatedOn DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO Changescripts (CurrentVersion,PreviousVersion,ChangescriptDescription) SELECT '1', '0', 'Initialize';
";

        public static ISessionFactory Configure()
        {
            var xmlConfig = new Configuration();
            xmlConfig.Configure();

            var sessionFactory = Fluently.Configure(xmlConfig)
                .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Bootstrapper>().Conventions.AddFromAssemblyOf<Bootstrapper>())
                .ExposeConfiguration(ConfigureDatabaseIfNeeded)
                .BuildSessionFactory();

            return sessionFactory;
        }

        private static void ConfigureDatabaseIfNeeded(Configuration obj)
        {
            string connectionString;
            if (!obj.Properties.TryGetValue("connection.connection_string", out connectionString))
                throw new InvalidOperationException("Connection string not found.");

            CreateTableIfNeeded(connectionString);

            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                ChangescriptsHandler.ApplyChangescripts(dbConnection);
                dbConnection.Close();
            }
        }

        private static void CreateTableIfNeeded(string connectionString)
        {
            var regex = Regex.Match(connectionString, "Data Source=(.*?)(;|$)");
            if (!regex.Success)
                throw new InvalidOperationException($"Invalid connection string {connectionString}");

            var dbFileName = regex.Groups[1].Value;
            if (File.Exists(dbFileName))
                return;

            SQLiteConnection.CreateFile(dbFileName);
            using (var dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();
                new SQLiteCommand(CreateChangescriptTableQuery, dbConnection).ExecuteNonQuery();
                dbConnection.Close();
            }
        }
    }
}
