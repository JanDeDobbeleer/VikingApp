﻿using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Settings
{
    public partial class Settings : PhoneApplicationPage
    {
        private IEnumerable<Sim> _sims;
        //private readonly List<String> _topupValues = new List<string> { "10", "15", "25", "40", "60" };
        //private readonly List<string> _tileValues = new List<string> { "phone theme", "viking red" };
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private bool _starting;

        public Settings()
        {
            _starting = true;
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            _starting = true;
            //TileColorPicker.ItemsSource = _tileValues.ToList();
            TileColorPicker.SelectedIndex = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] ? 0 : 1;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            ShowHideSimTopup((bool)IsolatedStorageSettings.ApplicationSettings["topup"]);
            _starting = false;
            _cts = new CancellationTokenSource();
            await GetSimInfo(_cts);
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            _cts.Cancel();
        }

        public async Task<bool> GetSimInfo(CancellationTokenSource cts)
        {
            Tools.Tools.SetProgressIndicator(true);
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
                    _sims = JsonConvert.DeserializeObject<Sim[]>(args.Json);
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            ShowHideDefaultSim(_sims.Count() > 1);
        }

        private void ShowHideSimTopup(bool on)
        {
            Switch.IsChecked = on;
            //ReloadPicker.ItemsSource = _topupValues.ToList();
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
                case "40":
                    return 3;
                case "60":
                    return 4;
                default:
                    return 0;
            }
        }

        private void ShowHideDefaultSim(bool on)
        {
            foreach (var item in _sims.Select(sim => new ListPickerItem {Content = sim.msisdn, Style = ItemStyle}))
            {
                item.Style = ItemStyle;
                SimPicker.Items.Add(item);
            }
            //SimPicker.ItemsSource = _sims.Select(x => x.msisdn);
            var visible = (on) ? Visibility.Visible : Visibility.Collapsed;
            SimText.Visibility = visible;
            SimPicker.Visibility = visible;
            SimCheck.Visibility = (_sims.Count() == 1) ? visible : Visibility.Visible;
            LastUsedText.Visibility = (_sims.Count() == 1) ? visible : Visibility.Visible;
        }

        private void Picker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListPicker) == null || _starting || ((sender as ListPicker).SelectedItem as ListPickerItem) == null)
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
                    var pickerItem3 = (sender as ListPicker).SelectedItem as ListPickerItem;
                    if (pickerItem3 != null && !string.IsNullOrWhiteSpace(pickerItem3.Content.ToString()))
                        IsolatedStorageSettings.ApplicationSettings["sim"] = pickerItem3.Content.ToString();
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

        private void Logout_OnClick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Logout", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            IsolatedStorageSettings.ApplicationSettings["login"] = true;
            Tools.Tools.DefaultAllSettings();
            NavigationService.GoBack();
            //NavigationService.Navigate(new Uri("/Fuel;component/View/MainPivot.xaml", UriKind.Relative));
        }
    }
}