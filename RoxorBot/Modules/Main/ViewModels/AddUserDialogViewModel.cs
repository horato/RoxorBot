using System;
using System.Collections;
using Prism.Mvvm;
using RoxorBot.Data.Attributes;
using RoxorBot.Data.Base;
using RoxorBot.Data.Enums;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Dialog;
using RoxorBot.Data.Model.Wrappers;

namespace RoxorBot.Modules.Main.ViewModels
{
    public class AddUserDialogViewModel : ViewModelBase, IDialogViewModel
    {
        private readonly IUsersManager _usersManager;
        private UserWrapper _selectedUser;

        public Action Close { get; set; }
        public IEnumerable Roles { get; } = Enum.GetValues(typeof(Role));
        public string Name { get { return GetValue<string>(); } set { SetValue(value); } }
        public Role Role { get { return GetValue<Role>(); } set { SetValue(value); } }
        public int Points { get { return GetValue<int>(); } set { SetValue(value); } }
        public bool IsAllowed { get { return GetValue<bool>(); } set { SetValue(value); } }

        public AddUserDialogViewModel(IUsersManager usersManager)
        {
            _usersManager = usersManager;
        }

        public void SetData(object data)
        {
            var user = data as UserWrapper;
            if (user == null)
                return;

            _selectedUser = user;
            Name = user.VisibleName;
            Role = user.Role;
            Points = user.Points;
            IsAllowed = user.IsAllowed;
        }

        [Command]
        public void Add()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return;
            if (Role == default(Role))
                return;

            var points = Points < 0 ? 0 : Points;
            if (_selectedUser == null)
                _usersManager.AddNewUser(Name, Role, false, points, false, null, IsAllowed);
            else
                _usersManager.UpdateUser(_selectedUser.Id, Name, Role, _selectedUser.IsOnline, points, _selectedUser.IsFollower, _selectedUser.IsFollowerSince, IsAllowed);

            Close?.Invoke();
        }
    }
}
