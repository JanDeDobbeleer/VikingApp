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
    public class MainPivotViewmodel : CancelAsyncTask, INotifyPropertyChanged
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

        #region event handling
        public event GetInfoFinishedEventHandler GetBalanceInfoFinished;

        protected void OnGetBalanceInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetBalanceInfoFinished != null)
            {
                GetBalanceInfoFinished(this, args);
            }
        }

        public event GetInfoFinishedEventHandler GetSimInfoFinished;

        protected void OnGetSimInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetBalanceInfoFinished != null)
            {
                GetSimInfoFinished(this, args);
            }
        }
        #endregion

        public MainPivotViewmodel()
        {
            Balance = new UserBalance();
        }

        public async Task<bool> GetData(string msisdn)
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching data";
            var client = new VikingsApi();
            client.GetInfoFinished += client_GetDataFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Balance, new KeyValuePair { name = "msisdn", content = msisdn }, Cts);
            return true;
        }

        void client_GetDataFinished(object sender, GetInfoCompletedArgs args)
        {
            switch (args.Canceled)
            {
                case true:
                    Tools.Tools.SetProgressIndicator(false);
                    break;
                case false:
                    if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                        return;
                    Balance.Load(args.Json);
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            OnGetBalanceInfoFinished(args);
        }

        public async Task<bool> GetSimInfo()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "loading sims";
            var client = new VikingsApi();
            client.GetInfoFinished += client_GetSimInfoFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Sim, new KeyValuePair { content = "1", name = "alias" }, Cts);
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
                    Sims = JsonConvert.DeserializeObject<Sim[]>(args.Json);
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            OnGetSimInfoFinished(args);
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