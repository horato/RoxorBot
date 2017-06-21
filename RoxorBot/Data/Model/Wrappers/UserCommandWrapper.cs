using System;
using RoxorBot.Data.Model.Database.Entities;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Data.Model
{
    public class UserCommandWrapper : WrapperBase<UserCommand>
    {
        public Guid Id => Model.Id;
        public string Command { get { return GetValue<string>(); } set { SetValue(value); } }
        public string Reply { get { return GetValue<string>(); } set { SetValue(value); } }

        public UserCommandWrapper(UserCommand model) : base(model)
        {
        }
    }
}
