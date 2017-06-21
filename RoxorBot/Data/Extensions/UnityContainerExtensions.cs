using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using RoxorBot.Data.Implementations;

namespace RoxorBot.Data.Extensions
{
    public static class UnityContainerExtensions
    {
        public static TViewModel ResolveViewModel<TViewModel>(this IUnityContainer container)
        {
            var vm = ViewModelProvider.CreateViewModel<TViewModel>();
            return (TViewModel)container.Resolve(vm);
        }
    }
}
