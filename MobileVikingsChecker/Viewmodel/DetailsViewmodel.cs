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

        /// <summary>
        /// Get the usage for a certain period of time
        /// </summary>
        /// <param name="msisdn">msisdn number</param>
        /// <param name="from">date from when to start</param>
        /// <param name="to">date where to end</param>
        /// <returns>Sets the Usage property of the Viewmodel</returns>
        public async Task<bool> GetUsage(string msisdn, DateTime from, DateTime to)
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
            //TODO: format string accordingly
            string json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Usage, new[] { new KeyValuePair { content = msisdn, name = "msisdn" }, new KeyValuePair { content = to.ToVikingApiTimeFormat(), name = "until_date" }, new KeyValuePair { content = from.ToVikingApiTimeFormat(), name = "from_date" } });

            if (string.IsNullOrEmpty(json))
                return false;

            Usage = JsonConvert.DeserializeObject<Usage[]>(json);
            Tools.Tools.SetProgressIndicator(false);
            return true;
        }
    }
}
