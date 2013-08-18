using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MobileVikingsChecker.Common;

namespace MobileVikingsChecker.View
{
    public partial class Login : PhoneApplicationPage
    {
        #region variables
        const string ConsumerKey = "un9HyMLftXRtDf89jP";
        const string ConsumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
        private VikingsClient _client;
        private string PinUrl;
        private bool _startNavigating;
        #endregion

        public Login()
        {
            InitializeComponent();
        }

        #region buttons
        private async void Login_OnTap(object sender, RoutedEventArgs e)
        {
            SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "Getting ready to loot and plunder";
            await Task.Run(() => GetPinUri());
            PinBrowser.Navigated += PinBrowserOnNavigated;
            SystemTray.ProgressIndicator.Text = "Sharpening the knife";
            PinBrowser.Navigate(new Uri(PinUrl));
            _startNavigating = true;
        }

        private void Check_OnTap(object sender, GestureEventArgs e)
        {
            //TODO: continue here
            RevertToLogin(false);
        }

        #region tasks
        public async Task GetPinUri()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            PinUrl = await VikingsClient.GetPinRequestUrl(ConsumerKey, ConsumerSecret);
        }
        #endregion
        #endregion

        #region progressindicator
        private void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
        }

        static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }
        #endregion

        #region browser
        private void PinBrowserOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            if (navigationEventArgs.Uri.AbsolutePath.Contains("sims"))
            {
                PinBrowser.Navigate(new Uri(PinUrl));
            }
        }

        private void PinBrowser_OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!_startNavigating) return;
            SystemTray.ProgressIndicator.Text = "Let's go!";
            LoginBtn.Visibility = Visibility.Collapsed;
            PincodeGrid.Visibility = Visibility.Visible;
            PinBrowser.Visibility = Visibility.Visible;
            SetProgressIndicator(false);
            _startNavigating = false;
        }
        #endregion

        #region functions

        private void RevertToLogin(bool isLoginEnabled)
        {
            if (!isLoginEnabled)
            {
                LoginBtn.Tap -= Login_OnTap;
                LoginBtn.Text = "welcome";
            }
            LoginBtn.Visibility = Visibility.Visible;
            PincodeGrid.Visibility = Visibility.Collapsed;
            PinBrowser.Visibility = Visibility.Collapsed;
        }
        #endregion
    }
}