using System;
using System.Windows;
using System.Windows.Input;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddPointsDialog 
    {
        public AddPointsDialog()
        {
            InitializeComponent();
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
    }
}
