using System;
using System.Windows;

namespace RoxorBot.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RaffleView
    {
        public RaffleView()
        {
            InitializeComponent();
        }
        
        private void RaffleView_OnClosed(object sender, EventArgs e)
        {
            var disposable = DataContext as IDisposable;
            disposable?.Dispose();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
