using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MahApps.Metro.Controls;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Data.Base
{
    public class DialogBase : MetroWindow, IDialog
    {
        public void SetViewModel(IDialogViewModel viewModel)
        {
            if (viewModel == null)
                return;

            viewModel.Close = Close;
            DataContext = viewModel;
        }
    }
}
