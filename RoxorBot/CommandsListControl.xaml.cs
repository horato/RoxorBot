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
    /// Interaction logic for CommandsListControl.xaml
    /// </summary>
    public partial class CommandsListControl : UserControl
    {
        //why not?
        private List<string> commands = new List<string>()
        {
            "!addfilter slovo cas",
            "!whitelist slovo",
            "!removefilter slovo",
            "!removewhitelist slovo",
            "!allow nick",
            "!unallow nick",
            "!points nick",
            "!addpoints nick pocet",
            "!removepoints nick pocet",
            "!since",
            "!uptime",
            "!isfollower nick",
            "!gettimer nick - interni timer pro body za 30m",
            "!isallowed nick",
            "!addcomm command reply",
            "!delcomm command (zatim vypnuto)"
        };

        public CommandsListControl()
        {
            InitializeComponent();

            commands.ForEach(x => CommandsListView.Items.Add(x));
        }
    }
}
