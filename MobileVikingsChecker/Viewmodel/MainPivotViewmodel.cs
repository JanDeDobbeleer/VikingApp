using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using Tools.Annotations;
using VikingApi.ApiTools;
using VikingApi.AppClasses;
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    public class MainPivotViewmodel:INotifyPropertyChanged
    {
        #region properties
        private IEnumerable<Sim> _sims;
        public IEnumerable<Sim> Sims
        {
            get { return _sims; }
            set
            {
                if (value.Equals(_sims))
                    return;
                _sims = value;
                OnPropertyChanged();
            }
        }
        private UserBalance _balance;
        public UserBalance Balance
        {
            get { return _balance; }
            set
            {
                if (value.Equals(_balance))
                    return;
                _balance = value;
                OnPropertyChanged();
            }
        }
        #endregion

        public MainPivotViewmodel()
        {
            Balance = new UserBalance();
        }

        public async Task<bool> GetData(string msisdn)
        {
            try
            {
                Tools.Tools.SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "fetching data";
                var client = new VikingsApi();
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                string json = await client.GetInfo(new AccessToken((string) IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string) IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Balance, new KeyValuePair {name = "msisdn", content = msisdn});
                if (Error.HandleError(json, "there seems to be no connection"))
                    return false;
                Tools.Tools.SetProgressIndicator(false);
                Balance.Load(json);
                return true;
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load bundle info, please try again later");
                return false;
            }
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
                Sims = JsonConvert.DeserializeObject<Sim[]>(json);
                Tools.Tools.SetProgressIndicator(false);
                return true;
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load sim information, please try again later");
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}