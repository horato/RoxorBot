using System;
using Prism.Mvvm;
using RoxorBot.Data.Enums;

namespace RoxorBot.Data.Model.Wrappers
{
    /// <summary> Display wrapper for the <see cref="UserWrapper"/> class </summary>
    public class UserDisplayWrapper : BindableBase
    {
        private UserWrapper _model;
        private string _name;
        private bool _displayAsModerator;

        public string Name => _displayAsModerator ? $"(o) {_name}" : _name;
        public Role Role { get; private set; }

        public UserDisplayWrapper(UserWrapper model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            _model = model;
            _name = _model.VisibleName;
            Role = _model.Role;
        }

        public void DisplayAsModerator()
        {
            _displayAsModerator = true;
            RaisePropertyChanged(nameof(Name));
        }
    }
}
