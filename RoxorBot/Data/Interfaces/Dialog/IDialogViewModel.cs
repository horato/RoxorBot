using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoxorBot.Data.Interfaces.Dialog
{
    public interface IDialogViewModel
    {
        Action Close { get; set; }
        void SetData(object data);
    }
}
