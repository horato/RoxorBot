using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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
    public partial class YoutubeWindow : Window
    {
        public bool close = false;
        private dynamic videoPlayer;

        public YoutubeWindow()
        {
            if (!WebCore.IsInitialized)
                WebCore.Initialize(new WebConfig()
                {
                    RemoteDebuggingHost = "127.0.0.1",
                    RemoteDebuggingPort = 2229,
                    LogLevel = LogLevel.Verbose
                });
            InitializeComponent();

            browser.Loaded += browser_Loaded;
        }

        void browser_Loaded(object sender, RoutedEventArgs e)
        {
            browser.DocumentReady += browser_DocumentReady;
            browser.Source = new Uri(YoutubeManager.getInstance().getVideoDirectLink("oHg5SJYRHA0"));
        }

        void browser_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            browser.DocumentReady -= browser_DocumentReady;
            browser.ExecuteJavascript("var player = document.getElementsByTagName(\"video\")[0];");
            videoPlayer = (Awesomium.Core.JSObject)browser.ExecuteJavascriptWithResult("player");
            browser.ExecuteJavascript("function playSong(url) { player.src = url; player.load(); player.play(); }");
            JSObject jsobject = browser.CreateGlobalJavascriptObject("VideoCallback");

            jsobject.Bind("getNextSongUrl", sasd);
        }

        private JSValue sasd(JSValue[] arguments)
        {
            foreach(var a in arguments)
            System.Diagnostics.Debug.Write(a+" ");
            System.Diagnostics.Debug.WriteLine("");
            return "";
        }



        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !close;
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            browser.ExecuteJavascript("playSong('" + YoutubeManager.getInstance().getVideoDirectLink("RSdKmX2BH7o") + "');");
        }
    }
}
