using RoxorBot.Data.Constants;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Model.Database.Mappings
{
    public class FilterMap : EntityMap<Filter>
    {
        public FilterMap()
        {
            Map(x => x.Word).Unique();
            Map(x => x.BanDuration).Not.Nullable();
            Map(x => x.Author).Not.Nullable();
            Map(x => x.IsRegex).Not.Nullable().Default("0");
            Map(x => x.IsWhitelist).Not.Nullable().Default("0");
        }

        public override string GetTableName()
        {
            return SqlTableNames.Filters;
        }
    }
}
