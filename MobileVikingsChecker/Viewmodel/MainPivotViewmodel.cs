using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.ApiTools;
using VikingApi.AppClasses;
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    internal class MainPivotViewmodel
    {
        public IEnumerable<Sim> Sims;
 
        public async Task<UserBalance> GetData(string msisdn)
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
                    return null;
                Tools.Tools.SetProgressIndicator(false);
                return new UserBalance(json);
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load bundle info, please try again later");
                return null;
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
    }
}