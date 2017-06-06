using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoxorBot
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddMessageDialog
    {
        public int id = 0;
        public bool active = true;

        public AddMessageDialog()
        {
            InitializeComponent();
        }

        private void IntervalBox_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(Char.IsNumber(Convert.ToChar(e.Text)) && (IntervalBox.Text.Length < 6));
        }

        private void IntervalBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = (e.Key == Key.Space);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
