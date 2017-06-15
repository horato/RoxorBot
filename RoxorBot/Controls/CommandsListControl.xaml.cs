using System.Collections.Generic;
using System.Windows;

namespace RoxorBot.Controls
{
    /// <summary>
    /// Interaction logic for CommandsListControl.xaml
    /// </summary>
    public partial class CommandsListControl
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
            "!delcomm command (zatim vypnuto)",
            "!songrequest link",
            "!skipsong",
            "!volume 1-100",
            "!notifyNextSong on/off - zapne/vypne hlasky o nasledujicim songu"
        };

        public CommandsListControl()
        {
            InitializeComponent();

            commands.ForEach(x => CommandsListView.Items.Add(x));
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
