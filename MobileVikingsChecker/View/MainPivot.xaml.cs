using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Fuel.LoginControl;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Tools;
using VikingApi.Json;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        private bool _logout;

        public MainPivot()
        {
            InitializeComponent();
            BuildApplicationBar();
            DataContext = App.Viewmodel.MainPivotViewmodel.Balance;
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
        }

        private async void RefreshOnClick(object sender, EventArgs e)
        {
            if (await SetData(SimBox.Text))
                Loading.Begin();
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            var task = new SmsComposeTask { To = "8989", Body = string.Format("sim topup {0}", (string)IsolatedStorageSettings.ApplicationSettings["defaulttopupvalue"]) };
            task.Show();
        }

        private void SettingsOnClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Fuel.Settings;component/Settings.xaml", UriKind.Relative));
        }

        private void OnClickLogout(object sender, EventArgs e)
        {
            Tools.Tools.DefaultAllSettings();
            _logout = true;
            SimBox.Text = string.Empty;
            IsolatedStorageSettings.ApplicationSettings["login"] = true;
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            ShowLogin();
        }

        #region startup

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationBar.MenuItems.Clear();
            if ((bool) IsolatedStorageSettings.ApplicationSettings["topup"])
            {
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("sms topup", true, ReloadOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("settings", true, SettingsOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("logout", true, OnClickLogout));
            }
            else
            {
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("settings", true, SettingsOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("logout", true, OnClickLogout));
            }
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            if ((bool)IsolatedStorageSettings.ApplicationSettings["login"])
            {
                ShowLogin();
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                if (await GetData() && e.NavigationMode == NavigationMode.Back)
                    Loading.Begin();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["lastusedsim"])
                Tools.Tools.SaveSetting(new KeyValuePair { name = "sim", content = SimBox.Text });
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
            if (await GetData() && _logout)
            {
                Loading.Begin();
                _logout = false;
            }
        }

        private async Task<bool> GetData()
        {
            await SetSimList();
            await SetData(SimBox.Text);
            return true;
        }

        private async Task<bool> SetData(string msisdn)
        {
            if (!await App.Viewmodel.MainPivotViewmodel.GetData(msisdn)) 
                return false;
            Bundle.Visibility = Visibility.Visible;
            Bonus.Visibility = Visibility.Visible;
            return true;
        }

        private async Task<bool> SetSimList()
        {
            if (!await App.Viewmodel.MainPivotViewmodel.GetSimInfo()) 
                return false;
            if (App.Viewmodel.MainPivotViewmodel.Sims != null && App.Viewmodel.MainPivotViewmodel.Sims.Any())
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("sim"))
                {
                    SimBox.Text = CheckDefaultSimValue();
                    return true;
                }
                SimBox.Text = App.Viewmodel.MainPivotViewmodel.Sims.Select(x => x.msisdn).FirstOrDefault();
                return true;
            }
            Message.ShowToast("it seems like there is no sim linked to this account");
            return false;
        }

        private string CheckDefaultSimValue()
        {
            if (App.Viewmodel.MainPivotViewmodel.Sims.Where(x => x.msisdn == (string) IsolatedStorageSettings.ApplicationSettings["sim"]).Any())
            {
                if (App.Viewmodel.MainPivotViewmodel.Sims.Count() > 1)
                    Tools.Message.ShowToast("default sim does not exist anymore, loading first from list");
                return (string)IsolatedStorageSettings.ApplicationSettings["sim"];
            }
            return App.Viewmodel.MainPivotViewmodel.Sims.Select(x => x.msisdn).FirstOrDefault();
        }

        #endregion

        private void SimBox_OnTap(object sender, GestureEventArgs e)
        {
            if (App.Viewmodel.MainPivotViewmodel.Sims.Count() == 1)
                return;
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            SimBox.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = false;
            LongListSelector.ItemsSource = App.Viewmodel.MainPivotViewmodel.Sims.ToList();
            LongListSelector.Visibility = Visibility.Visible;
        }

        private async void LongListSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LongListSelector.SelectedItem as Sim == null)
                return;
            SimBox.Text = (LongListSelector.SelectedItem as Sim).msisdn;
            LongListSelector.Visibility = Visibility.Collapsed;
            SimBox.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = true;
            await SetData(SimBox.Text);
        }

        private void Bundle_OnTap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Views/DetailsPage?msisdn={0}", SimBox.Text), UriKind.Relative));
        }
    }
}