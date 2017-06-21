using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Interfaces;

namespace RoxorBot.Data.Implementations.Database.Changescripts
{
    public static class ChangescriptsHandler
    {
        private static readonly List<IChangescript> _changescripts = new List<IChangescript>();

        static ChangescriptsHandler()
        {
            var types = typeof(ChangescriptsHandler).Assembly.DefinedTypes;
            var changescripts = types.Where(x => typeof(IChangescript).IsAssignableFrom(x) && !x.IsAbstract && x.IsClass);
            var changescriptsWithAttribute = changescripts.Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(ChangescriptAttribute)));
            var changescriptsWithParameterlessConstructor = changescriptsWithAttribute.Where(x => x.GetConstructors().Any(y => !y.GetParameters().Any()));
            foreach (var changescript in changescriptsWithParameterlessConstructor)
            {
                var instance = Activator.CreateInstance(changescript) as IChangescript;
                if (instance == null)
                    continue;

                _changescripts.Add(instance);
            }
        }

        public static void ApplyChangescripts(SQLiteConnection connection)
        {
            var scripts = _changescripts.Where(x => x.GetAttribute<ChangescriptAttribute>() != null).OrderBy(x => x.GetAttribute<ChangescriptAttribute>().SourceVersion).ToList();
            foreach (var script in scripts)
            {
                var scalar = new SQLiteCommand("SELECT CurrentVersion FROM Changescripts order by Id desc LIMIT 1;", connection).ExecuteScalar();
                int currentVersion;
                if (!int.TryParse(scalar?.ToString(), out currentVersion))
                    throw new InvalidOperationException($"{scalar} is not an int");

                var attr = script.GetAttribute<ChangescriptAttribute>();
                if (attr.SourceVersion != currentVersion)
                    continue;

                var tran = connection.BeginTransaction(IsolationLevel.Serializable);
                try
                {
                    script.Execute(connection);
                    new SQLiteCommand($"INSERT INTO Changescripts (CurrentVersion, PreviousVersion, ChangescriptDescription) VALUES ({attr.TargetVersion}, {attr.SourceVersion}, '{attr.Description}');", connection).ExecuteNonQuery();
                }
                catch
                {
                    tran.Rollback();
                    throw;
                }
                tran.Commit();
            }
        }

        private static T GetAttribute<T>(this IChangescript script) where T : class
        {
            if (script == null)
                return null;

            return script.GetType().GetCustomAttribute(typeof(T)) as T;
        }
    }
}
