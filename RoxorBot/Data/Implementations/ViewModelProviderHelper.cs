using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;

namespace RoxorBot.Data.Implementations
{
    public static class ViewModelProviderHelper
    {
        private static readonly List<WeakReference> ViewModels = new List<WeakReference>();

        public static void RaiseCanExecuteChanged(object instance)
        {
            if (instance == null)
                return;

            var commands = instance.GetType().GetProperties().Where(x => x.PropertyType == typeof(ICommand));
            foreach (var command in commands)
            {
                var delegateCommand = command.GetValue(instance) as DelegateCommandBase;
                delegateCommand?.RaiseCanExecuteChanged();

            }
        }

        public static void RaiseAllCanExecuteChanged()
        {
            var instances = GetAliveInstances();
            foreach (var instance in instances)
            {
                RaiseCanExecuteChanged(instance);
            }
        }

        public static void RegisterViewModel(object instance)
        {
            if (ViewModels.Any(x => x.Target == instance))
                return;

            ViewModels.Add(new WeakReference(instance));
        }

        public static void UnregisterViewModel(object instance)
        {
            var obj = ViewModels.Single(x => x.Target == instance);
            if (obj != null)
                ViewModels.Remove(obj);
        }

        public static List<object> GetAliveInstances()
        {
            ViewModels.RemoveAll(x => !x.IsAlive);
            return ViewModels.Select(x => x.Target).ToList();
        }
    }
}
