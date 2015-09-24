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
            browser.Source = new Uri(getVideoUrl("oHg5SJYRHA0"));
            //browser.Source = new Uri("http://youtube.com/watch?v=jnslrTTXQSA");
        }
        private string getVideoUrl(string id = "oHg5SJYRHA0")
        {
            var map = getFmtMap(id);
            string signature = "";

            if (map.ContainsKey("sig"))
            {
                signature = "&signature=" + map["sig"];
            }
            else if (map.ContainsKey("s"))
            {
                if (map.ContainsKey("js"))
                {
                    signature = "&signature=" + js_descramble(map["s"], map["js"]);
                }
            }
            if (map.ContainsKey("url"))
            {
                return map["url"] + signature;
            }

            return "";
        }

        private string js_descramble(string sig, string js_url)
        {
            using (var client = new WebClient())
            {
                var js = client.DownloadString(js_url);
                if (string.IsNullOrEmpty(js))
                    return sig;

                var reg = Regex.Match(js, "set\\(\"signature\",.*?(.*?)\\(");
                var descrambler = "";
                if (reg.Success)
                    descrambler = reg.Groups[1].Value;


                string transformations = "", rules = "";

                reg = Regex.Match(js, "var ..={(.*?)};function " + descrambler + "\\([^)]*\\){(.*?)}");
                if (reg.Success)
                {
                    transformations = reg.Groups[1].Value;
                    rules = reg.Groups[2].Value;
                }

                var trans = new Dictionary<string, string>();
                reg = Regex.Match(transformations, "(..):function\\([^)]*\\){([^}]*)}");
                while (reg.Success)
                {
                    var meth = reg.Groups[1].Value;
                    var code = reg.Groups[2].Value;

                    if (Regex.IsMatch(code, "\\.reverse\\("))
                        trans.Add(meth, "reverse");
                    else if (Regex.IsMatch(code, "\\.splice\\("))
                        trans.Add(meth, "slice");
                    else if (Regex.IsMatch(code, "var c="))
                        trans.Add(meth, "swap");
                    else
                        System.Diagnostics.Debug.WriteLine("Couldn't parse unknown youtube video URL signature transformation");

                    reg = reg.NextMatch();
                }

                var missing = false;
                reg = Regex.Match(rules, "..\\.(..)\\([^,]+,(\\d+)\\)");
                while (reg.Success)
                {
                    var meth = reg.Groups[1].Value;
                    var idx = int.Parse(reg.Groups[2].Value);

                    if (trans[meth] == "reverse")
                    {
                        char[] charArray = sig.ToCharArray();
                        Array.Reverse(charArray);
                        sig = new string(charArray);
                    }
                    else if (trans[meth] == "slice")
                    {
                        sig = sig.Substring(idx);
                    }
                    else if (trans[meth] == "swap")
                    {
                        if (idx > 1)
                        {
                            string replicate = "";
                            for (int i = 1; i < idx; i++)
                                replicate += ".";
                            //signature = string.gsub( sig, "^(.)("..string.rep( ".", idx - 1 )..")(.)(.*)$", "%3%2%1%4" )
                            var reg2 = Regex.Match(sig, "^(.)(" + replicate + ")(.)(.*)$");
                            if (reg2.Success)
                            {
                                var first = reg2.Groups[1].Value;
                                var second = reg2.Groups[2].Value;
                                var third = reg2.Groups[3].Value;
                                var fourth = reg2.Groups[4].Value;
                                sig = third + second + first + fourth;
                            }
                            else
                                throw new Exception("Failed to reverse 2");
                        }
                        else if (idx == 1)
                        {
                            var reg2 = Regex.Match(sig, "^(.)(.)");
                            if (reg2.Success)
                            {
                                var first = reg2.Groups[1].Value;
                                var second = reg2.Groups[2].Value;
                                sig = Regex.Replace(sig, "^" + first + second, second + first);
                            }
                            else
                                throw new Exception("Failed to reverse");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Couldn't apply unknown youtube video URL signature transformation");
                            missing = true;
                        }
                    }
                    reg = reg.NextMatch();
                }


                if (missing)
                    System.Diagnostics.Debug.WriteLine("Couldn't process youtube video URL, please check for updates to this script");

                return sig;
            }
        }

        private Dictionary<string, string> getFmtMap(string id)
        {
            var result = new Dictionary<string, string>();
            using (WebClient client = new WebClient())
            {
                var info = client.DownloadString("http://www.youtube.com/watch?v=" + id);
                var regex = Regex.Match(info, "\"url_encoded_fmt_stream_map\":\"(.*?)\"");
                if (regex.Success)
                {
                    var map = regex.Groups[1].Value.Replace("\\u0026", "&").Split(',');
                    var m = map[0];

                    var values = m.Split('&');
                    foreach (var kvp in values)
                    {
                        var str = kvp.Split('=');
                        result.Add(str[0], HttpUtility.UrlDecode(str[1]));
                    }
                }

                regex = Regex.Match(info, "\"js\":\"(.*?)\"");
                if (regex.Success)
                {
                    var js_url = HttpUtility.UrlDecode(regex.Groups[1].Value);
                    js_url = js_url.Replace("\\/", "/");
                    js_url = Regex.Replace(js_url, "^//", "http://");
                    result.Add("js", js_url);
                }
            }
            return result;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !close;
            Hide();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dynamic document = (Awesomium.Core.JSObject)browser.ExecuteJavascriptWithResult("document");

        }
    }
}
