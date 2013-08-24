using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncOAuth;
using Coding4Fun.Toolkit.Controls;
using Fuel.Common;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using VikingApi.ApiTools;

namespace Fuel.Controls
{
    public delegate void BrowserFinishedEventHandler(object sender, ApiBrowserEventArgs args);

    public partial class ApiBrowser : UserControl
    {
        public event BrowserFinishedEventHandler BrowserFinished;
        public string PinUrl { get; set; }
        public WebBrowser Browser { get { return WebBrowser; } set { WebBrowser = value; } }

        public ApiBrowser()
        {
            InitializeComponent();
        }

        protected void OnBrowserFinished(ApiBrowserEventArgs args)
        {
            if (BrowserFinished != null)
            {
                BrowserFinished(this, args);
            }
        }

        private void AvoidPebcak(object sender, NavigatingEventArgs navigatingEventArgs)
        {
            //stop navigation if the user selects something he or she shouldn't navigate to (avoid pebcak)
            switch (navigatingEventArgs.Uri.AbsolutePath)
            {
                case "/api/2.0/oauth/authorize/":
                case "/bel/en/account/login/":
                case "/account/login/":
                    break;
                case "/bel/sims/":
                    Browser.Navigate(new Uri(PinUrl));
                    break;
                default:
                    navigatingEventArgs.Cancel = true;
                    break;
            }
        }

        private async void BrowserOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            if (!navigationEventArgs.Uri.AbsoluteUri.Equals("https://mobilevikings.com/api/2.0/oauth/authorize/"))
                return;
            SystemTray.ProgressIndicator.Text = "boarding the ship";
            WebBrowser.Visibility = Visibility.Collapsed;

            // first define a new function which returns the content of "code" as string
            Browser.InvokeScript("eval", "this.newfunc_getmyvalue = function() { return document.getElementsByClassName('code')[0].innerHTML; }");
            // invoke the function and save the result
            var pin = (string)Browser.InvokeScript("newfunc_getmyvalue");

            //fire correct event to show result
            var args = new ApiBrowserEventArgs();
            args.Success = await GetAccessToken(pin);
            OnBrowserFinished(args);
        }

        private async Task<bool> GetAccessToken(string pincode)
        {
            bool success = false;
            try
            {
                AccessToken accesstoken;
                //try connecting to the service
                for (int i = 1; (i <= 3) && !success; i++)
                {
                    accesstoken = await VikingsClient.GetAccessToken(pincode);
                    if (accesstoken != null)
                    {
                        //try saving the token
                        for (int j = 1; (j <= 3) && !success; j++)
                        {
                            success = Tools.SaveSetting(new KeyValuePair() { name = "accesstoken", content = accesstoken });
                        }
                    }
                }
                return success;
            }
            catch (Exception)
            {
                return success;
            }
        }

        public void GoToPinUrl()
        {
            WebBrowser.Navigate(new Uri(PinUrl));
        }
    }
}
