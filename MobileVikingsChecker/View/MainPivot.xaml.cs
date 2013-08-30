using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using AsyncOAuth;
using Fuel.LoginControl;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Tools;
using VikingApi.ApiTools;
using VikingApi.AppClasses;
using VikingApi.Json;

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
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/euro.png", "reload", true, ReloadOnClick));
            
            var appBarMenuItem = new ApplicationBarMenuItem();
            appBarMenuItem.Text = "logout";
            appBarMenuItem.Click += OnClickLogout;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private async void RefreshOnClick(object sender, EventArgs e)
        {
            await GetData(SimBox.SelectedItem.ToString());
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            ReloadMenu.Visibility = ReloadMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            if((sender as TextBlock) == null)
                return;
            var task = new SmsComposeTask { To = "8989", Body = string.Format("sim topup {0}",(sender as TextBlock).Text) };
            ReloadMenu.Visibility = Visibility.Collapsed;
            task.Show();
        }

        private void OnClickLogout(object sender, EventArgs e)
        {
            IsolatedStorageSettings.ApplicationSettings["login"] = true;
            bundleGrid.Visibility = Visibility.Collapsed;
            bonusGrid.Visibility = Visibility.Collapsed;
            ShowLogin();
        }

        #region startup
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            bundleGrid.Visibility = Visibility.Collapsed;
            bonusGrid.Visibility = Visibility.Collapsed;
            if ((bool)IsolatedStorageSettings.ApplicationSettings["login"])
            {
                ShowLogin();
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                SimBox.ItemsSource = ((Sim[])IsolatedStorageSettings.ApplicationSettings["siminfo"]).Select(x=>x.msisdn);
                await LoadData(SimBox.Items[0].ToString());
                await Task.Run(() => LoadSims());
            }
        }

        private void ShowLogin()
        {
            var login = new OauthLogin();
            ApplicationBar.IsVisible = false;
            Pivot.Visibility = Visibility.Collapsed;
            login.LoginFinished += OauthLoginFinished;
            LayoutRoot.Children.Add(login);
        }

        private async void OauthLoginFinished(object sender, ApiBrowserEventArgs args)
        {
            Pivot.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = true;
            LayoutRoot.Children.Remove(LayoutRoot.Children.First(c => c.GetType() == typeof(OauthLogin)));
            SimBox.ItemsSource = ((Sim[])IsolatedStorageSettings.ApplicationSettings["siminfo"]).Select(x => x.msisdn);
            await LoadData(SimBox.Items[0].ToString());
            Tools.Tools.SetProgressIndicator(false);
        }

        private async Task<bool> LoadData(string msisdn)
        {
            if (await GetData(msisdn))
            {
                bundleGrid.Visibility = Visibility.Visible;
                bonusGrid.Visibility = Visibility.Visible;
            }
            return true;
        }

        private async Task<bool> GetData(string msisdn)
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching data";
            var client = new VikingsClient();
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            string json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Balance, new KeyValuePair{name = "msisdn", content = msisdn});
            if (Tools.Error.HandleError(json, "there seems to be no connection"))
                return false;
            DataContext = new UserBalance(json);
            return true;
        }

        private async void LoadSims()
        {
            SystemTray.ProgressIndicator.Text = "loading sims";
            var client = new VikingsClient();
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };
            string json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Balance, new KeyValuePair { content = "1", name = "alias" });
            
            // if new json equals one in storage, discard
            if (!json.Equals((string) IsolatedStorageSettings.ApplicationSettings["siminfo"]))
            {
                Tools.Tools.SaveSetting(new KeyValuePair { name = "siminfo", content = json });
                SimBox.ItemsSource = ((Sim[])IsolatedStorageSettings.ApplicationSettings["siminfo"]).Select(x => x.msisdn);
            }
            Tools.Tools.SetProgressIndicator(false);
        }
        #endregion
    }
}
