using System;
using System.Windows;
using System.Windows.Input;
using RoxorBot.Data.Interfaces.Dialog;

namespace RoxorBot.Controls
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

        private void PointsTextBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsNumber(Convert.ToChar(e.Text)));
        }

        private void PointsTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Space);
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
