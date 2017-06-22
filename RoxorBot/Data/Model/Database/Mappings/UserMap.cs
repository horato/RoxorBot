using RoxorBot.Data.Constants;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Model.Database.Mappings
{
    public class UserMap : EntityMap<User>
    {
        public UserMap()
        {
            Map(x => x.VisibleName).Not.Nullable();
            Map(x => x.ValueName).Not.Nullable().Unique();
            Map(x => x.Role).Not.Nullable();
            Map(x => x.IsOnline).Not.Nullable();
            Map(x => x.Points).Not.Nullable();
            Map(x => x.IsFollower).Not.Nullable();
            Map(x => x.IsFollowerSince);
            Map(x => x.IsAllowed).Not.Nullable();
        }

        public override string GetTableName()
        {
            return SqlTableNames.Users;
        }
    }
}