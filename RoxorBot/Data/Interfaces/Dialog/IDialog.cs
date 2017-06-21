using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Data.Interfaces
{
    public interface IDialog
    {
        void SetViewModel(IDialogViewModel viewModel);
        bool? ShowDialog();
    }
}
