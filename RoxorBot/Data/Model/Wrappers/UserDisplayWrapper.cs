using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RoxorBot.Data.Model
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
