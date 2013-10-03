using System;
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
        private readonly List<String> _topupValues = new List<string>{ "10", "15", "25", "40", "60" };
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public Settings()
        {
            InitializeComponent();

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            ShowHideSimTopup((bool)IsolatedStorageSettings.ApplicationSettings["topup"]);
            if (await GetSimInfo(_cts))
            {
                ShowHideDefaultSim(_sims.Count() > 1);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _cts.Cancel();
        }

        public async Task<bool> GetSimInfo(CancellationTokenSource cts)
        {
            Tools.Tools.SetProgressIndicator(true);
            try
            {
                SystemTray.ProgressIndicator.Text = "loading sims";
                var client = new VikingsApi();
                client.GetInfoFinished += client_GetInfoFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Sim, new KeyValuePair { content = "1", name = "alias" }, _cts);
                return true;
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load sim information, please try again later");
                return false;
            }
        }

        void client_GetInfoFinished(object sender, GetInfoCompletedArgs args)
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
        }

        private void ShowHideSimTopup(bool on)
        {
            Switch.IsChecked = on;
            ReloadPicker.ItemsSource = _topupValues.ToList();
            var visible = (on) ? Visibility.Visible : Visibility.Collapsed;
            TopupText.Visibility = visible;
            ReloadPicker.Visibility = visible;
        }

        private void ShowHideDefaultSim(bool on)
        {
            SimPicker.ItemsSource = _sims.Select(x=>x.msisdn);
            var visible = (on) ? Visibility.Visible : Visibility.Collapsed;
            SimText.Visibility = visible;
            SimPicker.Visibility = visible;
            SimCheck.Visibility = (_sims.Count() == 1)? visible:Visibility.Visible;
            LastUsedText.Visibility = (_sims.Count() == 1) ? visible : Visibility.Visible;
        }

        private void Picker_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListPicker) == null)
                return;
            if (!string.IsNullOrWhiteSpace((string)(sender as ListPicker).SelectedItem))
                Tools.Tools.SaveSetting(new KeyValuePair { name = (sender as ListPicker).Tag.ToString(), content = (sender as ListPicker).SelectedItem });
        }

        private void SimCheck_OnChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox) == null)
                return;
            Tools.Tools.SaveSetting(new []
            {
                new KeyValuePair { name = "lastusedsim", content = (sender as CheckBox).IsChecked ?? false },
                new KeyValuePair {name = "sim", content = string.Empty }
            });
            ShowHideDefaultSim(!((sender as CheckBox).IsChecked ?? false) && _sims.Count() > 1);
        }

        private void Switch_OnChecked(object sender, RoutedEventArgs e)
        {
            if ((sender as ToggleSwitch) == null)
                return;
            Tools.Tools.SaveSetting(new KeyValuePair { name = "topup", content = (sender as ToggleSwitch).IsChecked ?? false });
            ShowHideSimTopup((sender as ToggleSwitch).IsChecked ?? false);
        }
    }
}