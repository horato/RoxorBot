using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RoxorBot.Data.Base
{
    public abstract class ViewModelBase : BindableBase
    {
        private Dictionary<string, object> _propertyDictionary;

        protected ViewModelBase()
        {
            _propertyDictionary = new Dictionary<string, object>();
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (!_propertyDictionary.ContainsKey(propertyName))
                return default(T);

            var value = _propertyDictionary[propertyName];
            if (value == null)
                return default(T);
            if (!(value is T))
                throw new InvalidOperationException($"{this}: Property {propertyName} requested invalid value type ({typeof(T)}, actual type is {value.GetType()})");

            return (T)_propertyDictionary[propertyName];
        }

        protected void SetValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (_propertyDictionary.ContainsKey(propertyName))
                _propertyDictionary[propertyName] = value;
            else
                _propertyDictionary.Add(propertyName, value);

            RaisePropertyChanged(propertyName);
        }
    }
}
