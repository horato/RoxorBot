using RoxorBot.Data.Constants;

namespace RoxorBot.Data.Model.Database.Mappings
{
    public class AutomatedMessageMap : EntityMap<Entities.AutomatedMessage>
    {
        public AutomatedMessageMap()
        {
            Map(x => x.Message).Not.Nullable();
            Map(x => x.Interval).Not.Nullable();
            Map(x => x.Enabled).Not.Nullable().Default("1");
        }

        public override string GetTableName()
        {
            return SqlTableNames.Messages;
        }
    }
}
