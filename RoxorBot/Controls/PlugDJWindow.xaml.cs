using Awesomium.Core;
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
    /// Interaction logic for PlugDJWindow.xaml
    /// </summary>
    public partial class PlugDJWindow : Window
    {
        public bool close = false;

        public PlugDJWindow()
        {
            InitializeComponent();

            browser.Loaded += browser_Loaded;
            browser.LoadingFrameComplete += doLogin;
        }

        void browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Source = new Uri("http://plug.dj/");
        }

        //TROLOLOLOLOL
        void doLogin(object sender, Awesomium.Core.FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                browser.LoadingFrameComplete -= doLogin;
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.plugdjLogin) || string.IsNullOrWhiteSpace(Properties.Settings.Default.plugdjLogin))
                    return;

                browser.LoadingFrameComplete += switchToCommunity;
                browser.ExecuteJavascript("document.getElementById(\"email\").value = \"" + Properties.Settings.Default.plugdjLogin + "\";");
                browser.ExecuteJavascript("document.getElementById(\"password\").value= \"" + Properties.Settings.Default.plugdjPassword + "\";");
                browser.ExecuteJavascript("document.getElementsByClassName(\"email-login\")[0].getElementsByTagName(\"button\")[0].click();");
            }
        }

        void switchToCommunity(object sender, Awesomium.Core.FrameEventArgs e)
        {
            if (e.IsMainFrame)
            {
                browser.LoadingFrameComplete -= switchToCommunity;
                System.Threading.Thread.Sleep(5000);
                browser.Source = new Uri("https://plug.dj/roxorova-%C5%BEalu%C4%8F-p%C3%A1rty/");
                browser.ProcessInput = ViewInput.All;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !close;
            Hide();
        }
    }
}
