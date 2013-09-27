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
        public string Msisdn;
        public IEnumerable<Usage> Usage;

        public async Task<bool> GetUsage(DateTime fromDate, DateTime untilDate)
        {
            var pair = new[]
            {
                new KeyValuePair{content = Msisdn, name = VikingApi.Json.Usage.Msisdn},
                new KeyValuePair{content = fromDate.ToVikingApiTimeFormat(), name = VikingApi.Json.Usage.FromDate},
                new KeyValuePair{content = untilDate.ToVikingApiTimeFormat(), name = VikingApi.Json.Usage.UntilDate}
            };
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
            var json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Usage, pair);
            if (string.IsNullOrEmpty(json) || string.Equals(json, "[]"))
                return false;
            Usage = JsonConvert.DeserializeObject<Usage[]>(json);
            Tools.Tools.SetProgressIndicator(false);
            return true;
        }
    }
}
