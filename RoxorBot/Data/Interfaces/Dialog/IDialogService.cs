namespace RoxorBot.Data.Interfaces.Dialog
{
    public interface IDialogService
    {
        void ShowDialog<TView, TViewModel>() where TView : IDialog where TViewModel : IDialogViewModel;
        void ShowDialog<TView, TViewModel>(object data) where TView : IDialog where TViewModel : IDialogViewModel;
    }
}
