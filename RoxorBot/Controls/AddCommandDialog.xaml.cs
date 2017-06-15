using System.Windows;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddCommandDialog 
    {
        public int id = 0;

        public AddCommandDialog()
        {
            InitializeComponent();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
