using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    public class DetailsViewmodel
    {
        public IEnumerable<Usage> Usage;

        public async Task<bool> GetUsage(string msisdn)
        {
            Tools.Tools.SetProgressIndicator(true);
            try
            {
                Tools.Tools.SetProgressIndicator(true);
                SystemTray.ProgressIndicator.Text = "retrieving information";
                var client = new VikingsApi();
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                string json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Usage, new KeyValuePair { content = msisdn, name = "msisdn" });
                Usage = JsonConvert.DeserializeObject<Usage[]>(json);
                Tools.Tools.SetProgressIndicator(false);
                return true;
            }
            catch (Exception)
            {
                Message.ShowToast("Could not load usage information, please try again later");
                return false;
            }
        }
    }
}
