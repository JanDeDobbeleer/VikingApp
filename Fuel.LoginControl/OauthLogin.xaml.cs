using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using Tools;
using VikingApi.ApiTools;

namespace Fuel.LoginControl
{
    public partial class OauthLogin : UserControl
    {
        private bool _startNavigating;

        public OauthLogin()
        {
            InitializeComponent();
        }

        public event BrowserFinishedEventHandler LoginFinished;

        protected void OnLoginFinished(ApiBrowserEventArgs args)
        {
            if (LoginFinished != null)
            {
                LoginFinished(this, args);
            }
        }

        private void OauthLogin_OnLoaded(object sender, RoutedEventArgs e)
        {
            var v = (Visibility) Resources["PhoneLightThemeVisibility"];
            VikingImage.Source = (v == Visibility.Visible) ? new BitmapImage(new Uri("/Assets/vikinglogoblack.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/vikinglogowhite.png", UriKind.Relative));
        }

        #region buttons

        private async void Login_OnTap(object sender, RoutedEventArgs e)
        {
            //Get Pin URL
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "requesting secure access";
            await Task.Run(() => GetPinUri());

            //Check URL to see whether or not an error was thrown
            if (Error.HandleError(PinBrowser.PinUrl, "there seems to be no connection"))
                return;

            //Load pages in browser and login
            PinBrowser.Browser.LoadCompleted += PinBrowser_OnLoadCompleted;
            PinBrowser.BrowserFinished += PinBrowserOnBrowserFinished;
            SystemTray.ProgressIndicator.Text = "loading page";
            PinBrowser.GoToPinUrl();
            _startNavigating = true;
        }

        #region ApiTasks

        private async Task GetPinUri()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            PinBrowser.PinUrl = await VikingsApi.GetPinUrl();
        }

        #endregion

        #endregion

        #region PinBrowser

        private void PinBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!_startNavigating) return;
            LoginBtn.Visibility = Visibility.Collapsed;
            PinBrowser.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
            _startNavigating = false;
        }

        private async void PinBrowserOnBrowserFinished(object sender, ApiBrowserEventArgs args)
        {
            //fire correct event to show result
            if (args.Success)
            {
                //fire correct event to indicate everything has finished correctly
                var finished = new ApiBrowserEventArgs {Success = true};
                OnLoginFinished(finished);
                Visibility = Visibility.Collapsed;
            }
        }

        #endregion
    }
}