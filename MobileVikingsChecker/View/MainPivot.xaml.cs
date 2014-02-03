using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Fuel.Localization.Resources;
using Fuel.LoginControl;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Tools;
using VikingApi.Json;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        private bool _isLoginControlEnabled;
        private bool _loading;

        public MainPivot()
        {
            InitializeComponent();
            BuildApplicationBar();
            DataContext = App.Viewmodel.MainPivotViewmodel.Balance;
            App.Viewmodel.MainPivotViewmodel.GetBalanceInfoFinished += MainPivotViewmodel_GetBalanceInfoFinished;
            App.Viewmodel.MainPivotViewmodel.GetSimInfoFinished += MainPivotViewmodel_GetSimInfoFinished;
        }

        #region buttons
        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", AppResources.AppBarButtonRefresh, true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/feature.calendar.png", AppResources.AppBarButtonUsage, true, UsageOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/sim.png", AppResources.AppBarButtonSim, true, SimOnClick));
        }

        private async void RefreshOnClick(object sender, EventArgs e)
        {
            _loading = true;
            await App.Viewmodel.MainPivotViewmodel.GetData(string.IsNullOrWhiteSpace(SimBox.Text) ? (string)IsolatedStorageSettings.ApplicationSettings["sim"] : SimBox.Text);
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            var task = new SmsComposeTask { To = "8989", Body = string.Format("sim topup {0}", IsolatedStorageSettings.ApplicationSettings["defaulttopupvalue"]) };
            task.Show();
        }

        private void BalanceOnClick(object sender, EventArgs e)
        {
            var task = new SmsComposeTask { To = "1966 ", Body = "BAL" };
            task.Show();
        }

        private void SimOnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SimBox.Text) && string.IsNullOrWhiteSpace((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
            {
                Message.ShowToast(AppResources.ToastSimLoaded);
                return;
            }
            App.Viewmodel.MainPivotViewmodel.CancelTask();
            App.Viewmodel.SimViewmodel.Msisdn = SimBox.Text;
            NavigationService.Navigate(new Uri("/View/SimPage.xaml", UriKind.Relative));
        }

        private void AboutOnClick(object sender, EventArgs e)
        {
            if (!_loading)
            {
                App.Viewmodel.MainPivotViewmodel.CancelTask();
                NavigationService.Navigate(new Uri("/Fuel.About;component/About.xaml", UriKind.Relative));
            }
            else
            {
                Message.ShowToast(AppResources.ToastHoldHorses);
            }
        }

        private void SettingsOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.MainPivotViewmodel.CancelTask();
            NavigationService.Navigate(new Uri("/Fuel.Settings;component/Settings.xaml", UriKind.Relative));
        }

        private void ProfileOnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SimBox.Text) && string.IsNullOrWhiteSpace((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
            {
                Message.ShowToast(AppResources.ToastSimLoaded);
                return;
            }
            App.Viewmodel.MainPivotViewmodel.CancelTask();
            App.Viewmodel.ProfileViewmodel.Msisdn = string.IsNullOrWhiteSpace(SimBox.Text) ? (string)IsolatedStorageSettings.ApplicationSettings["sim"] : SimBox.Text;
            NavigationService.Navigate(new Uri("/View/ProfilePage.xaml", UriKind.Relative));
        }

        private void UsageOnClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SimBox.Text) && string.IsNullOrWhiteSpace((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
            {
                Message.ShowToast(AppResources.ToastSimLoaded);
                return;
            }
            App.Viewmodel.MainPivotViewmodel.CancelTask();
            App.Viewmodel.UsageViewmodel.Msisdn = string.IsNullOrWhiteSpace(SimBox.Text) ? (string)IsolatedStorageSettings.ApplicationSettings["sim"] : SimBox.Text;
            NavigationService.Navigate(new Uri("/View/DetailsPage.xaml", UriKind.Relative));
        }
        #endregion

        #region startup
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationBar.MenuItems.Clear();
            if ((bool)IsolatedStorageSettings.ApplicationSettings["topup"])
            {
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuSmsTopup, true, ReloadOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuSmsBalance, true, BalanceOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuProfile, true, ProfileOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuSettings, true, SettingsOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuAbout, true, AboutOnClick));
            }
            else
            {
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuSmsBalance, true, BalanceOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuProfile, true, ProfileOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuSettings, true, SettingsOnClick));
                ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuAbout, true, AboutOnClick));
            }
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            if ((bool)IsolatedStorageSettings.ApplicationSettings["login"])
            {
                if (!_isLoginControlEnabled)
                    ShowLogin();
            }
            else if (!Tools.Tools.HasInternetConnection())
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                Message.ShowToast(AppResources.ToastBummer);
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                App.Viewmodel.MainPivotViewmodel.RenewToken();
                _loading = true;
                await App.Viewmodel.MainPivotViewmodel.GetSimInfo();
            }
        }
        #endregion

        #region events
        async void MainPivotViewmodel_GetSimInfoFinished(object sender, VikingApi.ApiTools.GetInfoCompletedArgs args)
        {
            if (App.Viewmodel.MainPivotViewmodel.Sims != null && App.Viewmodel.MainPivotViewmodel.Sims.Any() && !args.Canceled && !(bool)IsolatedStorageSettings.ApplicationSettings["login"])
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("sim"))
                {
                    SimBox.Text = CheckDefaultSimValue((bool)IsolatedStorageSettings.ApplicationSettings["login"]);
                }
                if (string.IsNullOrEmpty((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
                    IsolatedStorageSettings.ApplicationSettings["sim"] = SimBox.Text;
                App.Viewmodel.MainPivotViewmodel.StartPeriodicAgent();
                App.Viewmodel.MainPivotViewmodel.RenewToken();
                await App.Viewmodel.MainPivotViewmodel.GetData(SimBox.Text);
            }
            else if (!args.Canceled)
            {
                Message.ShowToast(AppResources.ToastNoSim);
            }
            _loading = false;
        }

        void MainPivotViewmodel_GetBalanceInfoFinished(object sender, VikingApi.ApiTools.GetInfoCompletedArgs args)
        {
            if (!args.Canceled)
            {
                Bundle.Visibility = Visibility.Visible;
                Bonus.Visibility = Visibility.Visible;
                Loading.Begin();
            }
            if ((bool)IsolatedStorageSettings.ApplicationSettings["login"])
                IsolatedStorageSettings.ApplicationSettings["login"] = false;
            _loading = false;
        }

        private async void OauthLoginFinished(object sender, ApiBrowserEventArgs args)
        {
            Pivot.Visibility = Visibility.Visible;
            ApplicationBar.IsVisible = true;
            LayoutRoot.Children.Remove(LayoutRoot.Children.First(c => c.GetType() == typeof(OauthLogin)));
            _isLoginControlEnabled = false;
            App.Viewmodel.MainPivotViewmodel.RenewToken();
            await App.Viewmodel.MainPivotViewmodel.GetSimInfo();
        }
        #endregion

        #region functions
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Viewmodel.MainPivotViewmodel.CancelTask();
            if ((bool)IsolatedStorageSettings.ApplicationSettings["lastusedsim"] || (App.Viewmodel.MainPivotViewmodel.Sims != null && App.Viewmodel.MainPivotViewmodel.Sims.Count() == 1))
            {
                if (!string.IsNullOrWhiteSpace(SimBox.Text))
                    Tools.Tools.SaveSetting(new KeyValuePair { Name = "sim", Content = SimBox.Text });
            }
            SimBox.Text = string.Empty;
            _loading = false;
            if (e.NavigationMode == NavigationMode.Back)
                _isLoginControlEnabled = false;
        }

        private void ShowLogin()
        {
            _isLoginControlEnabled = true;
            var login = new OauthLogin();
            ApplicationBar.IsVisible = false;
            Pivot.Visibility = Visibility.Collapsed;
            login.LoginFinished += OauthLoginFinished;
            LayoutRoot.Children.Add(login);
        }

        private string CheckDefaultSimValue(bool login)
        {
            if (App.Viewmodel.MainPivotViewmodel.Sims.Any(x => x.msisdn == (string)IsolatedStorageSettings.ApplicationSettings["sim"]))
                return (string)IsolatedStorageSettings.ApplicationSettings["sim"];
#if(DEBUG)
            foreach (var sim in App.Viewmodel.MainPivotViewmodel.Sims)
            {
                Debug.WriteLine("Sim number: " + sim.msisdn);
            }
            Debug.WriteLine("Number " + (string)IsolatedStorageSettings.ApplicationSettings["sim"] + " not in list");
#endif
            if (App.Viewmodel.MainPivotViewmodel.Sims.Count() > 1 && !login && !string.IsNullOrWhiteSpace((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
                Message.ShowToast(AppResources.ToastDefaultSim);
            return App.Viewmodel.MainPivotViewmodel.Sims.Select(x => x.msisdn).FirstOrDefault();
        }

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
            await App.Viewmodel.MainPivotViewmodel.GetData(SimBox.Text);
        }
        #endregion
    }
}