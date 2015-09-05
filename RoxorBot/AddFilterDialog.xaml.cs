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
    public partial class AddFilterDialog : UserControl
    {
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
    }
}
