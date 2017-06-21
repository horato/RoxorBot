using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Data.Interfaces
{
    public interface IDialogService
    {
        void ShowDialog<TView, TViewModel>() where TView : IDialog where TViewModel : IDialogViewModel;
        void ShowDialog<TView, TViewModel>(object data) where TView : IDialog where TViewModel : IDialogViewModel;
    }
}
