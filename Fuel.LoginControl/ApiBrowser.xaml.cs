using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tools;
using VikingApi.ApiTools;

namespace Fuel.LoginControl
{
    public delegate void BrowserFinishedEventHandler(object sender, ApiBrowserEventArgs args);

    public partial class ApiBrowser : UserControl
    {
        public ApiBrowser()
        {
            InitializeComponent();
        }

        public string PinUrl { get; set; }

        public WebBrowser Browser
        {
            get { return LoginBrowser; }
            set { LoginBrowser = value; }
        }

        public event BrowserFinishedEventHandler BrowserFinished;

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
            try
            {
                if (!navigationEventArgs.Uri.AbsoluteUri.Equals("https://mobilevikings.com/api/2.0/oauth/authorize/"))
                    return;
                Tools.Tools.SetProgressIndicator(true);
                LoginBrowser.Visibility = Visibility.Collapsed;
                SystemTray.ProgressIndicator.Text = "requesting token";

                // first define a new function which returns the content of "code" as string
                Browser.InvokeScript("eval", "this.newfunc_getmyvalue = function() { return document.getElementsByClassName('code')[0].innerHTML; }");
                // invoke the function and save the result
                //TODO: add logic to send mail if this fails (no login possible)
                string pin;
                try
                {
                    pin = (string) Browser.InvokeScript("newfunc_getmyvalue");
                }
                catch (Exception e)
                {
                    Tools.Message.ShowToast("Please authorize the application to continue");
                    Tools.Tools.SetProgressIndicator(false);
                    LoginBrowser.Visibility = Visibility.Visible;
                    return;
                }

                LoginBrowser.Visibility = Visibility.Collapsed;
                //fire correct event to show result
                var args = new ApiBrowserEventArgs {Success = await GetAccessToken(pin)};
                await Browser.ClearCookiesAsync();
                OnBrowserFinished(args);
            }
            catch (Exception)
            {
                var args = new ApiBrowserEventArgs {Success = false};
                OnBrowserFinished(args);
            }
        }

        private async Task<bool> GetAccessToken(string pincode)
        {
            bool success = false;
            try
            {
                //try connecting to the service
                for (int i = 1; (i <= 3) && !success; i++)
                {
                    AccessToken accesstoken = await VikingsClient.GetAccessToken(pincode);
                    if (accesstoken != null)
                    {
                        //try saving the token
                        for (int j = 1; (j <= 3) && !success; j++)
                        {
                            success = Tools.Tools.SaveSetting(new[]
                            {
                                new KeyValuePair {name = "login", content = false},
                                new KeyValuePair {name = "tokenKey", content = accesstoken.Key},
                                new KeyValuePair {name = "tokenSecret", content = accesstoken.Secret}
                            });
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
            LoginBrowser.Navigate(new Uri(PinUrl));
        }
    }
}