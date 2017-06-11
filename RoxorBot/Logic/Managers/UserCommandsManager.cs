using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IrcDotNet;
using Prism.Events;
using RoxorBot.Data.Events;
using RoxorBot.Data.Interfaces;

namespace RoxorBot
{
    public class UserCommandsManager : IUserCommandsManager
    {
        private readonly IEventAggregator _aggregator;
        private readonly IDatabaseManager _databaseManager;

        private readonly List<UserCommand> _commands;
        public int CommandsCount => _commands.Count;

        public UserCommandsManager(IEventAggregator aggregator, IDatabaseManager databaseManager)
        {
            _aggregator = aggregator;
            _databaseManager = databaseManager;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing UserCommandsManager...");
            _commands = LoadCommands();
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + CommandsCount + " user commands from database.");
        }

        public void AddCommand(string command, string reply, int id = 0)
        {
            if (id > 0)
            {
                _databaseManager.ExecuteNonQuery("INSERT OR REPLACE INTO userCommands(id, command, reply) VALUES (" + id + ", '" + command.ToLower() + "', '" + reply + "');");
            }
            else
            {
                _databaseManager.ExecuteNonQuery("INSERT INTO userCommands(command, reply) VALUES ('" + command.ToLower() + "', '" + reply + "');");
                var reader = _databaseManager.ExecuteReader("SELECT last_insert_rowid()");
                if (!reader.Read())
                    return;
                id = reader.GetInt32(0);
            }

            var cmd = _commands.Find(x => x.Id == id);
            if (cmd == null)
            {
                _commands.Add(new UserCommand { Id = id, Command = command.ToLower(), Reply = reply });
            }
            else
            {
                cmd.Command = command;
                cmd.Reply = reply;
            }
        }

        public bool RemoveCommand(string command)
        {
            var reader = _databaseManager.ExecuteReader("select max(id) from userCommands where command='" + command + "';");
            if (!reader.Read())
                return false;
            var id = reader.GetInt32(0);

            RemoveCommand(id);

            return true;
        }

        public void RemoveCommand(int id)
        {
            _databaseManager.ExecuteNonQuery("DELETE FROM userCommands WHERE id=" + id + ";");
            _commands.RemoveAll(x => x.Id == id);
        }

        private bool CommandsContains(string msg)
        {
            return _commands.Exists(x => x.Command.ToLower() == msg.ToLower());
        }

        private List<UserCommand> LoadCommands()
        {
            var reader = _databaseManager.ExecuteReader("SELECT * FROM userCommands;");
            var temp = new List<UserCommand>();

            while (reader.Read())
            {
                temp.Add(new UserCommand
                {
                    Id = Convert.ToInt32(reader["id"]),
                    Command = (string)reader["command"],
                    Reply = (string)reader["reply"]
                });
            }
            return temp;
        }

        public List<UserCommand> GetAllCommands()
        {
            return _commands.ToList();
        }

        public bool IsUserCommand(string command)
        {
            return CommandsContains(command);
        }
    }
}
