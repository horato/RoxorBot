using System;

namespace RoxorBot.Data.Interfaces.Dialog
{
    public interface IDialogViewModel
    {
        Action Close { get; set; }
        void SetData(object data);
    }
}
