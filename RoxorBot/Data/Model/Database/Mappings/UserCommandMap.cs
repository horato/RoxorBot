using RoxorBot.Data.Constants;

namespace RoxorBot.Data.Model.Database.Mappings
{
    public class UserCommandMap : EntityMap<Entities.UserCommand>
    {
        public UserCommandMap()
        {
            Map(x => x.Command).Not.Nullable();
            Map(x => x.Reply).Not.Nullable();
        }

        public override string GetTableName()
        {
            return SqlTableNames.UserCommands;
        }
    }
}
