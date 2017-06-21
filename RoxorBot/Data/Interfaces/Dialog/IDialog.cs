namespace RoxorBot.Data.Interfaces.Dialog
{
    public interface IDialog
    {
        void SetViewModel(IDialogViewModel viewModel);
        bool? ShowDialog();
    }
}
