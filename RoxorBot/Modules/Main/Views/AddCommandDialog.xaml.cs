using System.Windows;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddCommandDialog : IDialog
    {
        public AddCommandDialog()
        {
            DataContextChanged += AddCommandDialog_DataContextChanged;
            InitializeComponent();
        }

        private void AddCommandDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IDialogViewModel;
            if (vm == null)
                return;

            DataContextChanged -= AddCommandDialog_DataContextChanged;
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
