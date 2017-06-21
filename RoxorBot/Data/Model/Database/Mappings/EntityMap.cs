using FluentNHibernate.Mapping;
using RoxorBot.Data.Model.Database.Entities;

namespace RoxorBot.Data.Model.Database.Mappings
{
    public abstract class EntityMap<TEntity> : ClassMap<TEntity> where TEntity : Entity
    {
        protected EntityMap()
        {
            Id(x => x.Id).GeneratedBy.GuidComb();
            SetTableName();
            Map(x => x.DbTimestamp).Not.Nullable().Generated.Always().CustomSqlType("DATETIME").Index("IDX_DbTimestamp");
        }

        private void SetTableName()
        {
            Table(GetTableName());
        }
        
        public virtual string GetTableName() { return typeof(TEntity).Name; }
    }
}
