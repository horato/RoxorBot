using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Model.Database.Entities
{
    public class User : Entity
    {
        public virtual string VisibleName { get; set; }
        public virtual string ValueName { get; set; }
        public virtual Role Role { get; set; } = Role.Viewers;
        public virtual bool IsOnline { get; set; }
        public virtual int Points { get; set; }
        public virtual bool IsFollower { get; set; }
        public virtual DateTime? IsFollowerSince { get; set; }
        public virtual bool IsAllowed { get; set; }

        //nhibernate
        public User()
        {
        }

        public User(string visibleName, string valueName, Role role, bool isOnline, int points, bool isFollower, DateTime? isFollowerSince, bool isAllowed)
        {
            VisibleName = visibleName;
            ValueName = valueName;
            Role = role;
            IsOnline = isOnline;
            Points = points;
            IsFollower = isFollower;
            IsFollowerSince = isFollowerSince;
            IsAllowed = isAllowed;
        }
    }
}
