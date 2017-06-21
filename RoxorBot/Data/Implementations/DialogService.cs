using Microsoft.Practices.Unity;
using RoxorBot.Data.Extensions;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Data.Implementations
{
    public class DialogService : IDialogService
    {
        private readonly IUnityContainer _container;

        public DialogService(IUnityContainer container)
        {
            _container = container;
        }

        public void ShowDialog<TView, TViewModel>()
            where TView : IDialog
            where TViewModel : IDialogViewModel
        {
            var viewModel = _container.ResolveViewModel<TViewModel>();
            var dialog = _container.Resolve<TView>();
            dialog.SetViewModel(viewModel);
            dialog.ShowDialog();
        }

        public void ShowDialog<TView, TViewModel>(object data)
            where TView : IDialog
            where TViewModel : IDialogViewModel
        {
            var viewModel = _container.ResolveViewModel<TViewModel>();
            viewModel.SetData(data);

            var dialog = _container.Resolve<TView>();
            dialog.SetViewModel(viewModel);

            dialog.ShowDialog();
        }
    }
}
