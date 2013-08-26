using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Fuel.Common;
using Microsoft.Phone.Tasks;
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

        private async void RefreshOnClick(object sender, EventArgs e)
        {
            await LoadData();
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            var task = new SmsComposeTask { To = "8989", Body = "sim topup 15" };
            task.Show();
        }

        private async Task<bool> LoadData()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching data";
            var client = new VikingsClient();
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            string json = await client.GetBalance(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]));
            if (Tools.Tools.HandleError(json, "viking, your internet is broken."))
                return false;
            DataContext = new UserBalance(json);
            Tools.Tools.SetProgressIndicator(false);
            return true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            var v = (Visibility)Resources["PhoneLightThemeVisibility"];
            bundleGrid.Visibility = Visibility.Collapsed;
            bonusGrid.Visibility = Visibility.Collapsed;
            if (await LoadData())
            {
                bundleGrid.Visibility = Visibility.Visible;
                bonusGrid.Visibility = Visibility.Visible;
            }
        }
    }
}