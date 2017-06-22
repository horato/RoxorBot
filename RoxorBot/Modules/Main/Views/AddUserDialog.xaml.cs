using System;
using System.Windows;
using System.Windows.Input;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddUserDialog : IDialog
    {
        public AddUserDialog()
        {
            DataContextChanged += AddUserDialog_DataContextChanged;
            InitializeComponent();
        }

        private void AddUserDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var vm = DataContext as IDialogViewModel;
            if (vm == null)
                return;

            DataContextChanged -= AddUserDialog_DataContextChanged;
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
