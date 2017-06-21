namespace RoxorBot.Data.Model.Database.Entities
{
    public class UserCommand : Entity
    {
        public virtual string Command { get; set; }
        public virtual string Reply { get; set; }

        //nhibernate
        public UserCommand()
        {
        }

        public UserCommand(string command, string reply)
        {
            Command = command;
            Reply = reply;
        }
    }
}
