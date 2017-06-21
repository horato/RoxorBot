using System.Windows;
using System.Windows.Input;

namespace RoxorBot.Data.ViewHelpers
{
    /// <summary>
    /// Interaction logic for EventToCommand.xaml
    /// </summary>
    public partial class EventToCommand
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public EventToCommand()
        {
            InitializeComponent();
        }
    }
}
