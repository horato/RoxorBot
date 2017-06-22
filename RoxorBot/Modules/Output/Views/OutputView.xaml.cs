using System.Windows.Controls;

namespace RoxorBot.Modules.Output.Views
{
    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl
    {
        private bool _autoScroll = true;

        public OutputView()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.ScrollToEnd();
        }

        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var viewer = sender as ScrollViewer;
            if (viewer == null)
                return;

            if (e.ExtentHeightChange == 0)
            {
                _autoScroll = viewer.VerticalOffset == viewer.ScrollableHeight;
            }

            if (_autoScroll && e.ExtentHeightChange != 0)
            {
                viewer.ScrollToVerticalOffset(viewer.ExtentHeight);
            }
        }
    }
}
