using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RoxorBot.Data.Model
{
    /// <summary> Display wrapper for the <see cref="User"/> class </summary>
    public class UserWrapper : BindableBase
    {
        private User _model;
        private string _name;
        private bool _displayAsModerator;

        public string Name => _displayAsModerator ? $"(o) {_name}" : _name;
        public Role Role { get; private set; }

        public UserWrapper(User model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            _model = model;
            _name = _model.Name;
            Role = _model.Role;
        }

        public void DisplayAsModerator()
        {
            _displayAsModerator = true;
            RaisePropertyChanged(nameof(Name));
        }
    }
}
