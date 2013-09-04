using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
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
        private readonly int[] _topupValues = { 10, 15, 25, 40, 60 };

        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if(await GetSimInfo())
                SimPicker.ItemsSource = _sims;
            ReloadPicker.ItemsSource = _topupValues;
        }

        public async Task<bool> GetSimInfo()
        {
            Tools.Tools.SetProgressIndicator(true);
            try
            {
                Tools.Tools.SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "loading sims";
                var client = new VikingsApi();
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                string json = await client.GetInfo(new AccessToken((string) IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string) IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Sim, new KeyValuePair {content = "1", name = "alias"});
                _sims = JsonConvert.DeserializeObject<Sim[]>(json);
                Tools.Tools.SetProgressIndicator(false);
                return true;
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load sim information, please try again later");
                return false;
            }
        }
    }
}