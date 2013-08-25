using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Fuel.Common;
using VikingApi.ApiTools;
using VikingApi.AppClasses;
using VikingApi.Classes;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        public MainPivot()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/add.png", "reload", true, ReloadOnClick));
        }

        private void RefreshOnClick(object sender, EventArgs e)
        {
            //TODO: add logic to fetch new data
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            //TODO: add logic to fetch new data
        }

        private async void MainPivot_OnLoaded(object sender, RoutedEventArgs e)
        {
            //fetch new balance
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching data";
            var client = new VikingsClient();
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            DataContext = new UserBalance(await client.GetBalance(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"])));
            Tools.Tools.SetProgressIndicator(false);
            statusGrid.Visibility = Visibility.Visible;
        }
    }
}