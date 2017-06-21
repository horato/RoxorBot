using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace RoxorBot.Data.Model.Wrappers
{
    public abstract class WrapperBase<TModel> : BindableBase
    {
        public TModel Model { get; private set; }

        protected WrapperBase(TModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            Model = model;
        }

        protected void SetValue(object value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var property = Model.GetType().GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property {propertyName} does not exist on {typeof(TModel)}");

            property.SetValue(Model, value);
            RaisePropertyChanged(propertyName);
        }

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            var property = Model.GetType().GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException($"Property {propertyName} does not exist on {typeof(TModel)}");
            if (!typeof(T).IsAssignableFrom(property.PropertyType))
                throw new InvalidOperationException($"Property {propertyName} is not of a type {typeof(T)}");

            return (T)property.GetValue(Model);
        }
    }
}
