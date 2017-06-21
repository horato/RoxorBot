using System;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Model
{
    public class UserWrapper : WrapperBase<User>
    {
        public Guid Id => Model.Id;
        public string Name { get { return GetValue<string>(); } set { SetValue(value); } }
        public string InternalName { get { return GetValue<string>(); } set { SetValue(value); } }
        public Role Role { get { return GetValue<Role>(); } set { SetValue(value); } }
        public bool IsOnline { get { return GetValue<bool>(); } set { SetValue(value); } }
        public int Points { get { return GetValue<int>(); } set { SetValue(value); } }
        public bool IsFollower { get { return GetValue<bool>(); } set { SetValue(value); } }
        public DateTime? IsFollowerSince { get { return GetValue<DateTime?>(); } set { SetValue(value); } }
        public bool IsAllowed { get { return GetValue<bool>(); } set { SetValue(value); } }

        public int RewardTimer { get; set; }

        public UserWrapper(User model) : base(model)
        {
        }
    }
}
