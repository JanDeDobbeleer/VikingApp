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
        private int _count = 1;

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
                case "/bel/en/account/password/reset/":
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
                var success = false;
                while (!success)
                {
                    var pin = string.Empty;
                    try
                    {
                        pin = (string) Browser.InvokeScript("newfunc_getmyvalue");
                    }
                    catch (SystemException e)
                    {
                        _count++;
                        BrowserOnNavigated(sender, navigationEventArgs);
                        if(_count != 5)
                            return;
                        Message.SendErrorEmail(e.Message + Environment.NewLine + e.InnerException, "Fuel at login screen, request token script");
                        Tools.Tools.SetProgressIndicator(false);
                        LoginBrowser.Visibility = Visibility.Visible;
                        LoginBrowser.GoBack();
                        return;
                    }
                    success = true;
                    LoginBrowser.Visibility = Visibility.Collapsed;

                    //fire correct event to show result
                    var args = new ApiBrowserEventArgs { Success = await GetAccessToken(pin) };
                    await Browser.ClearCookiesAsync();
                    OnBrowserFinished(args);
                }
            }
            catch (Exception)
            {
                var args = new ApiBrowserEventArgs { Success = false };
                OnBrowserFinished(args);
            }
        }

        private async Task<bool> GetAccessToken(string pincode)
        {
            try
            {
                AccessToken accesstoken;
                using (var client = new VikingsApi())
                {
                    accesstoken = await client.GetAccessToken(pincode);
                }
                if (accesstoken == null)
                    return false;
                //save the token
                Tools.Tools.SaveSetting(new[]
                        {
                            new KeyValuePair {Name = "login", Content = false},
                            new KeyValuePair {Name = "tokenKey", Content = accesstoken.Key},
                            new KeyValuePair {Name = "tokenSecret", Content = accesstoken.Secret}
                        });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void GoToPinUrl()
        {
            LoginBrowser.Navigate(new Uri(PinUrl));
        }
    }
}