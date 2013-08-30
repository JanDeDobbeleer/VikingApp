using System;
using System.Collections.Generic;
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
using VikingApi.AppClasses;

namespace Fuel.View
{
    public partial class MainPivot : PhoneApplicationPage
    {
        private readonly MainPivotViewmodel _viewmodel;

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
            await _viewmodel.GetData(SimBox.SelectedItem.ToString());
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            ReloadMenu.Visibility = ReloadMenu.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            if ((sender as TextBlock) == null)
                return;
            var task = new SmsComposeTask {To = "8989", Body = string.Format("sim topup {0}", (sender as TextBlock).Text)};
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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            bundleGrid.Visibility = Visibility.Collapsed;
            bonusGrid.Visibility = Visibility.Collapsed;
            if ((bool) IsolatedStorageSettings.ApplicationSettings["login"])
            {
                ShowLogin();
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Pivot.Visibility = Visibility.Visible;
                await GetData();
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
            LayoutRoot.Children.Remove(LayoutRoot.Children.First(c => c.GetType() == typeof (OauthLogin)));
            await GetData();
        }

        private async Task<bool> GetData()
        {
            await SetSimList();
            await SetDataContext(SimBox.Items[0].ToString());
            return true;
        }

        private async Task<bool> SetDataContext(string msisdn)
        {
            UserBalance data = await _viewmodel.GetData(msisdn);
            if (data != null)
            {
                DataContext = data;
                bundleGrid.Visibility = Visibility.Visible;
                bonusGrid.Visibility = Visibility.Visible;
            }
            return true;
        }

        private async Task<bool> SetSimList()
        {
            IEnumerable<string> sims = await _viewmodel.LoadSims();
            if (sims != null)
            {
                SimBox.ItemsSource = sims;
            }
            return true;
        }

        #endregion
    }
}