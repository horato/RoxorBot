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
        private readonly IChatManager _chatManager;
        private readonly IDatabaseManager _databaseManager;
        private readonly IUsersManager _usersManager;

        private readonly List<UserCommand> _commands;
        public int CommandsCount => _commands.Count;

        public UserCommandsManager(IEventAggregator aggregator, IChatManager chatManager, IDatabaseManager databaseManager, IUsersManager usersManager)
        {
            _aggregator = aggregator;
            _chatManager = chatManager;
            _databaseManager = databaseManager;
            _usersManager = usersManager;

            _aggregator.GetEvent<AddLogEvent>().Publish("Initializing UserCommandsManager...");
            _commands = LoadCommands();
            _aggregator.GetEvent<IrcMessageReceived>().Subscribe(ChatMessageReceived);
            _aggregator.GetEvent<AddLogEvent>().Publish("Loaded " + CommandsCount + " user commands from database.");
        }

        private void ChatMessageReceived(IrcRawMessageEventArgs e)
        {
            if (e.Message.Parameters.Count < 2)
                return;

            var msg = e.Message.Parameters[1];
            if (CommandsContains(msg))
            {
                List<UserCommand> commandList;
                lock (_commands)
                    commandList = _commands.FindAll(x => x.command.ToLower() == msg.ToLower());

                var num = new Random().Next(0, commandList.Count - 1);
                var command = commandList[num];
                _chatManager.SendChatMessage(command.reply);
            }
            else if (msg.StartsWith("!addcomm ") && _usersManager.IsAdmin(e.Message.Source.Name))
            {
                var sb = new StringBuilder();
                var split = msg.Split(' ');

                if (split.Length < 3)
                    return;

                var command = split[1];
                for (int i = 2; i < split.Length; i++)
                    sb.Append(split[i]);
                var reply = sb.ToString();

                AddCommand(command, reply);
                _chatManager.SendChatMessage(e.Message.Source.Name + ": Command " + command + " added.");
            }
            /* else if (msg.StartsWith("!delcomm ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
             {
                 var split = msg.Split(' ');
                 var command = split[1];

                 if (removeCommand(command))
                     mainWindow.sendChatMessage(e.Message.Source.Name + ": Command " + command + " removed.");
             }*/
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

            var cmd = _commands.Find(x => x.id == id);
            if (cmd == null)
            {
                _commands.Add(new UserCommand { id = id, command = command.ToLower(), reply = reply });
            }
            else
            {
                cmd.command = command;
                cmd.reply = reply;
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
            _commands.RemoveAll(x => x.id == id);
        }

        private bool CommandsContains(string msg)
        {
            return _commands.Exists(x => x.command.ToLower() == msg.ToLower());
        }

        private List<UserCommand> LoadCommands()
        {
            var reader = _databaseManager.ExecuteReader("SELECT * FROM userCommands;");
            var temp = new List<UserCommand>();

            while (reader.Read())
            {
                temp.Add(new UserCommand
                {
                    id = Convert.ToInt32(reader["id"]),
                    command = (string)reader["command"],
                    reply = (string)reader["reply"]
                });
            }
            return temp;
        }

        public List<UserCommand> GetAllCommands()
        {
            return _commands;
        }
    }
}
