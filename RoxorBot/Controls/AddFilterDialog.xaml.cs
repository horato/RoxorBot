using System;
using System.Windows;
using System.Windows.Input;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddFilterDialog 
    {
        public int id { get; internal set; }

        public AddFilterDialog()
        {
            InitializeComponent();
        }

        private void DurationBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Contains("-") && string.IsNullOrWhiteSpace(DurationBox.Text))
                e.Handled = false;
            else
                e.Handled = !Char.IsNumber(Convert.ToChar(e.Text));
        }

        private void DurationBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Space);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
