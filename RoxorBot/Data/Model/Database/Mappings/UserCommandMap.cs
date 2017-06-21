using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Constants;
using RoxorBot.Data.Model.Database.Entities;

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
