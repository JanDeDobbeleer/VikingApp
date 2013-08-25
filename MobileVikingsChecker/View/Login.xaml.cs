using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Phone.Media.Capture;
using AsyncOAuth;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Devices.Sensors;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Input;
using Fuel.Common;
using VikingApi.ApiTools;
using VikingApi.Classes;

namespace Fuel.View
{
    public partial class Login : PhoneApplicationPage
    {
        #region variables
        private string _pinUrl;
        private bool _startNavigating;
        #endregion

        public Login()
        {
            InitializeComponent();
        }

        #region buttons
        private async void Login_OnTap(object sender, RoutedEventArgs e)
        {
            //Get Pin URL
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "getting ready to loot and plunder";
            await Task.Run(() => GetPinUri());

            //Check URL to see whether or not an error was thrown
            if (Tools.Tools.HandleError(PinBrowser.PinUrl, "viking, your internet is broken.")) 
                return;

            //Load pages in browser and login
            PinBrowser.Browser.LoadCompleted += PinBrowser_OnLoadCompleted;
            PinBrowser.BrowserFinished += PinBrowserOnBrowserFinished;
            SystemTray.ProgressIndicator.Text = "sharpening the axe";
            PinBrowser.GoToPinUrl();
            _startNavigating = true;
        }

        #region ApiTasks
        private async Task GetPinUri()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            PinBrowser.PinUrl = await VikingsClient.GetPinUrl();
        }
        #endregion
        #endregion

        #region progressindicator
        private void Login_OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            var v = (Visibility)Resources["PhoneLightThemeVisibility"];
            VikingImage.Source = (v == System.Windows.Visibility.Visible) ? new BitmapImage(new Uri("/Assets/vikinglogoblack.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/vikinglogowhite.png", UriKind.Relative));
        }
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

        private void PinBrowserOnBrowserFinished(object sender, ApiBrowserEventArgs args)
        {
            NavigationService.Navigate(new Uri("/View/MainPivot.xaml", UriKind.Relative));
            Tools.Tools.SetProgressIndicator(false);
        }
        #endregion
    }
}