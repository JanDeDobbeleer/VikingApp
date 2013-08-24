using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Fuel.Common;
using VikingApi.ApiTools;
using VikingApi.Classes;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        private Balance _balance;

        public MainPivot()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.CreateButton("/Assets/add.png", "reload", true, ReloadOnClick));
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
            //get last balance from storage
            _balance = (Balance)IsolatedStorageSettings.ApplicationSettings["balance"];
            Credits.Text = string.Format("€{0}", _balance.credits);

            //fetch new balance
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching new data";
            var client = new VikingsClient();
            Balance.ConvertBalance(await client.GetBalance((AccessToken)IsolatedStorageSettings.ApplicationSettings["accesstoken"]));
            _balance = (Balance)IsolatedStorageSettings.ApplicationSettings["balance"];
            Tools.SetProgressIndicator(false);
        }
    }
}