using System;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Fuel.LoginControl;
using Fuel.Viewmodel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Tools;
using VikingApi.AppClasses;
using VikingApi.Json;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        private readonly MainPivotViewmodel _viewmodel;
        private bool _logout;

        public MainPivot()
        {
            InitializeComponent();
            BuildApplicationBar();
            _viewmodel = new MainPivotViewmodel();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar {Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true};
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/euro.png", "reload", true, ReloadOnClick));

            var appBarMenuItem = new ApplicationBarMenuItem {Text = "logout"};
            appBarMenuItem.Click += OnClickLogout;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private async void RefreshOnClick(object sender, EventArgs e)
        {
            HideReloadMenu();
            if(await SetDataContext(SimBox.Text))
                Loading.Begin();
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            ReloadMenu.Visibility = ReloadMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            HideReloadMenu();
            if ((sender as TextBlock) == null)
                return;
            var task = new SmsComposeTask {To = "8989", Body = string.Format("sim topup {0}", (sender as TextBlock).Text)};
            ReloadMenu.Visibility = Visibility.Collapsed;
            task.Show();
        }

        private void OnClickLogout(object sender, EventArgs e)
        {
            _logout = true;
            HideReloadMenu();
            IsolatedStorageSettings.ApplicationSettings["login"] = true;
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            ShowLogin();
        }

        private void HideReloadMenu()
        {
            if (ReloadMenu.Visibility == Visibility.Visible)
                ReloadMenu.Visibility = Visibility.Collapsed;
        }

        #region startup

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            if ((bool) IsolatedStorageSettings.ApplicationSettings["login"])
            {
                ShowLogin();
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                if(await GetData() && e.NavigationMode == NavigationMode.Back)
                    Loading.Begin();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            Tools.Tools.SaveSetting(new KeyValuePair{name = "sim", content = SimBox.Text});
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
            LayoutRoot.Children.Remove(LayoutRoot.Children.First(c => c.GetType() == typeof (OauthLogin)));
            if (await GetData() && _logout)
            {
                Loading.Begin();
                _logout = false;
            }
        }

        private async Task<bool> GetData()
        {
            await SetSimList();
            await SetDataContext(SimBox.Text);
            return true;
        }

        private async Task<bool> SetDataContext(string msisdn)
        {
            UserBalance data = await _viewmodel.GetData(msisdn);
            if (data != null)
            {
                DataContext = data;
                Bundle.Visibility = Visibility.Visible;
                Bonus.Visibility = Visibility.Visible;
            }
            return true;
        }

        private async Task<bool> SetSimList()
        {
            await _viewmodel.GetSimInfo();
            if (_viewmodel.Sims != null)
            {
                SimBox.Text = IsolatedStorageSettings.ApplicationSettings.Contains("sim") ? (string)IsolatedStorageSettings.ApplicationSettings["sim"] : _viewmodel.Sims.Select(x => x.msisdn).FirstOrDefault();
            }
            return true;
        }

        #endregion

        private void SimBox_OnTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_viewmodel.Sims.Count() == 1)
                return;
            Bundle.Visibility = Visibility.Collapsed;
            Bonus.Visibility = Visibility.Collapsed;
            SimBox.Visibility = Visibility.Collapsed;
            ApplicationBar.IsVisible = false;
            LongListSelector.ItemsSource = _viewmodel.Sims.ToList();
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
            await SetDataContext(SimBox.Text);
        }
    }
}