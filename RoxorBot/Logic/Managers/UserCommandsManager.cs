using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot
{
    class UserCommandsManager
    {
        private static UserCommandsManager _instance;
        private List<UserCommand> commands;

        private UserCommandsManager()
        {
            Logger.Log("Initializing UserCommandsManager...");
            commands = loadCommands();
            MainWindow.ChatMessageReceived += MainWindow_ChatMessageReceived;
        }

        void MainWindow_ChatMessageReceived(object sender, IrcDotNet.IrcRawMessageEventArgs e)
        {
            if (!(sender is MainWindow))
                return;

            var mainWindow = ((MainWindow)sender);
            if (commandsContains(e.Message.Parameters[1]))
            {
                List<UserCommand> commandList;
                lock (commands)
                    commandList = commands.FindAll(x => x.command.ToLower() == e.Message.Parameters[1].ToLower());

                var num = new Random().Next(0, commandList.Count - 1);
                var command = commandList[num];
                mainWindow.sendChatMessage(command.reply);
            }
            else if (e.Message.Parameters[1].StartsWith("!addcomm ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
            {
                var sb = new StringBuilder();
                var split = e.Message.Parameters[1].Split(' ');

                if (split.Length < 3)
                    return;

                var command = split[1];
                for (int i = 2; i < split.Length; i++)
                    sb.Append(split[i]);
                var reply = sb.ToString();

                addCommand(command, reply);
                mainWindow.sendChatMessage(e.Message.Source.Name + ": Command " + command + " added.");
            }
            /* else if (e.Message.Parameters[1].StartsWith("!delcomm ") && UsersManager.getInstance().isAdmin(e.Message.Source.Name))
             {
                 var split = e.Message.Parameters[1].Split(' ');
                 var command = split[1];

                 if (removeCommand(command))
                     mainWindow.sendChatMessage(e.Message.Source.Name + ": Command " + command + " removed.");
             }*/
        }

        public void addCommand(string command, string reply, int id = 0)
        {
            if (id > 0)
            {
                DatabaseManager.getInstance().executeNonQuery("INSERT OR REPLACE INTO userCommands(id, command, reply) VALUES (" + id + ", '" + command.ToLower() + "', '" + reply + "');");

            }
            else
            {
                DatabaseManager.getInstance().executeNonQuery("INSERT INTO userCommands(command, reply) VALUES ('" + command.ToLower() + "', '" + reply + "');");
                var reader = DatabaseManager.getInstance().executeReader("SELECT last_insert_rowid()");
                if (!reader.Read())
                    return;
                id = reader.GetInt32(0);
            }

            lock (commands)
            {
                var cmd = commands.Find(x => x.id == id);
                if (cmd == null)
                {
                    commands.Add(new UserCommand { id = id, command = command.ToLower(), reply = reply });
                    }
                else
                {
                    cmd.command = command;
                    cmd.reply = reply;
                }
            }
        }

        public bool removeCommand(string command)
        {
            var reader = DatabaseManager.getInstance().executeReader("select max(id) from userCommands where command='" + command + "';");
            if (!reader.Read())
                return false;
            var id = reader.GetInt32(0);

            removeCommand(id);

            return true;
        }

        public void removeCommand(int id)
        {
            DatabaseManager.getInstance().executeNonQuery("DELETE FROM userCommands WHERE id=" + id + ";");
            commands.RemoveAll(x => x.id == id);
        }

        private bool commandsContains(string msg)
        {
            lock (commands)
            {
                return commands.Exists(x => x.command.ToLower() == msg.ToLower());
            }
        }

        private List<UserCommand> loadCommands()
        {
            var reader = DatabaseManager.getInstance().executeReader("SELECT * FROM userCommands;");
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

        public static UserCommandsManager getInstance()
        {
            if (_instance == null)
                _instance = new UserCommandsManager();
            return _instance;
        }

        internal int getCommandsCount()
        {
            lock (commands)
                return commands.Count;
        }

        public List<UserCommand> getAllCommands()
        {
            return commands;
        }
    }
}
