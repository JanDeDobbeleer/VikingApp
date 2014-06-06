using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncOAuth;
using Fuel.Database.Database;
using Fuel.Localization.Resources;
using Fuel.Settings.Annotations;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.Api;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Settings
{
    public partial class Settings : PhoneApplicationPage, INotifyPropertyChanged
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _starting;
        private bool _isPressed;

        public const string SimsPropertyName = "Sims";
        private ObservableCollection<string> _sims = new ObservableCollection<string>();
        public ObservableCollection<string> Sims
        {
            get
            {
                return _sims;
            }

            set
            {
                _sims = value;
                OnPropertyChanged(SimsPropertyName);
            }

        }

        public Settings()
        {
            _starting = true;
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Minimized, Opacity = 1, IsVisible = true };
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuReloadSims, true, SimRefresh_OnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuLogout, true, Logout_OnClick));
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _starting = true;
            TileColorPicker.SelectedIndex = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] ? 0 : 1;
            SmallTilePicker.SelectedIndex = (bool)IsolatedStorageSettings.ApplicationSettings["oldtilestyle"] ? 1 : 0;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            ShowHideSimTopup((bool)IsolatedStorageSettings.ApplicationSettings["topup"]);
            _cts = new CancellationTokenSource();
            await GetSimInfo(_cts);
        }

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            _cts.Cancel();
        }

        public async Task<bool> GetSimInfo(CancellationTokenSource cts, bool force = false)
        {
            Tools.Tools.SetProgressIndicator(true);
            if (CheckForSimsInDb() && !force)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() => ShowHideDefaultSim(Sims.Count() > 1));
                _starting = false;
                Tools.Tools.SetProgressIndicator(false);
                return true;
            }
            SystemTray.ProgressIndicator.Text = "loading sims";
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetSimInfoFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Sim, new KeyValuePair { Content = "1", Name = "alias" }, _cts);
            }
            return true;
        }

        private bool CheckForSimsInDb()
        {
            var db = new FuelDatabase();
            var value = !db.Sims.Any();
            if (value)
                return false;
            Sims.Clear();
            foreach (var sim in db.Sims)
            {
                Sims.Add(sim.Msisdn);
            }
            return true;
        }

        void client_GetSimInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            switch (args.Canceled)
            {
                case true:
                    Tools.Tools.SetProgressIndicator(false);
                    break;
                case false:
                    if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                        return;
                    try
                    {
                        var sims = JsonConvert.DeserializeObject<Sim[]>(args.Json);
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            Sims.Clear();
                            foreach (var sim in sims)
                            {
                                Sims.Add(sim.Msisdn);
                            }
                            ShowHideDefaultSim(Sims.Count() > 1);
                            _starting = false;
                        });
                        
                    }
                    catch (Exception e)
                    {
                        Tools.Tools.SetProgressIndicator(false);
                    }
                    break;
            }
        }

        private void ShowHideSimTopup(bool on)
        {
            Switch.IsChecked = on;
            var item = IsolatedStorageSettings.ApplicationSettings["defaulttopupvalue"].ToString();
            ReloadPicker.SelectedIndex = SetIndex(item);
            var visible = (on) ? Visibility.Visible : Visibility.Collapsed;
            TopupText.Visibility = visible;
            ReloadPicker.Visibility = visible;
        }

        private int SetIndex(string topUpValue)
        {
            switch (topUpValue)
            {
                case "15":
                    return 1;
                case "25":
                    return 2;
                case "50":
                    return 3;
                default:
                    return 0;
            }
        }

        private void ShowHideDefaultSim(bool on)
        {
            var visible = (on) ? Visibility.Visible : Visibility.Collapsed;
            SimText.Visibility = visible;
            SimPicker.Visibility = visible;
            SimCheck.Visibility = (_sims.Count() == 1) ? visible : Visibility.Visible;
            LastUsedText.Visibility = (_sims.Count() == 1) ? visible : Visibility.Visible;
            var number = IsolatedStorageSettings.ApplicationSettings["sim"].ToString();
            var index = 0;
            if (Sims.Contains(number))
            {
                index = Sims.IndexOf(number);
            }
            SimPicker.SelectedIndex = index;
            Tools.Tools.SetProgressIndicator(false);
        }

        private void Picker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListPicker) == null || _starting)
                return;
            switch ((sender as ListPicker).Tag.ToString())
            {
                case "defaulttopupvalue":
                    var listPickerItem = (sender as ListPicker).SelectedItem as ListPickerItem;
                    if (listPickerItem != null && !string.IsNullOrWhiteSpace(listPickerItem.Content.ToString()))
                        Tools.Tools.SaveSetting(new KeyValuePair { Name = (sender as ListPicker).Tag.ToString(), Content = listPickerItem.Content });
                    break;
                case "defaulttilevalue":
                    var pickerItem = (sender as ListPicker).SelectedItem as ListPickerItem;
                    if (pickerItem != null)
                        IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] = string.Equals(pickerItem.Content.ToString(), "theme");
                    Tools.Tools.ResetLiveTile();
                    break;
                case "sim":
                    var pickerItem3 = (sender as ListPicker).SelectedItem as String;
                    if (pickerItem3 != null && !string.IsNullOrWhiteSpace(pickerItem3) && !pickerItem3.Equals(IsolatedStorageSettings.ApplicationSettings["sim"].ToString()))
                    {
                        IsolatedStorageSettings.ApplicationSettings["sim"] = pickerItem3;
                        Tools.Tools.UpdateLiveTile();
                    }
                    break;
                case "smalltilestyle":
                    var pi = (sender as ListPicker).SelectedItem as ListPickerItem;
                    if (pi != null)
                        IsolatedStorageSettings.ApplicationSettings["oldtilestyle"] = string.Equals(pi.Content.ToString(), "circle");
                    Tools.Tools.ResetLiveTile();
                    break;
            }
        }

        private void SimCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox) == null || _starting)
            {
                Message.ShowToast("Please wait until the sim information is loaded");
                return;
            }
            Tools.Tools.SaveSetting(new[]
            {
                new KeyValuePair { Name = "lastusedsim", Content = (sender as CheckBox).IsChecked ?? false },
                new KeyValuePair {Name = "sim", Content = string.Empty }
            });
            ShowHideDefaultSim(!((sender as CheckBox).IsChecked ?? false) && _sims.Count() > 1);
        }

        private void Switch_OnChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch) == null || _starting)
                return;
            Tools.Tools.SaveSetting(new KeyValuePair { Name = "topup", Content = (sender as ToggleSwitch).IsChecked ?? false });
            ShowHideSimTopup((sender as ToggleSwitch).IsChecked ?? false);
        }

        private void Logout_OnClick(object sender, EventArgs e)
        {
            if (_isPressed)
                return;
            _isPressed = true;
            if (MessageBox.Show("Are you sure?", "Logout", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
            {
                _isPressed = false;
                return;
            }
            IsolatedStorageSettings.ApplicationSettings["login"] = true;
            var db = new FuelDatabase();
            db.DeleteAllSims();
            Tools.Tools.DefaultAllSettings();
            NavigationService.GoBack();
        }

        private async void SimRefresh_OnClick(object sender, EventArgs e)
        {
            await GetSimInfo(_cts, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if  (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}