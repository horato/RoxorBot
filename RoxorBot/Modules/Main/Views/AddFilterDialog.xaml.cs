using System.Windows;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddFilterDialog : IDialog
    {
        public AddFilterDialog()
        {
            DataContextChanged += AddMessageDialog_DataContextChanged;
            InitializeComponent();
        }

        private void AddMessageDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IDialogViewModel;
            if (vm == null)
                return;

            DataContextChanged -= AddMessageDialog_DataContextChanged;
            vm.Close = Close;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void SetViewModel(IDialogViewModel viewModel)
        {
            DataContext = viewModel;
        }
    }
}
