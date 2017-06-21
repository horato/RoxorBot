using System;
using System.Windows;
using System.Windows.Input;
using RoxorBot.Data.Interfaces;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddMessageDialog : IDialog
    {
        public AddMessageDialog()
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
