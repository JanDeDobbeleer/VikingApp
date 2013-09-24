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
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    public class DetailsViewmodel
    {
        public IEnumerable<Usage> Usage;

        /// <summary>
        /// Get the usage for a certain period of time
        /// </summary>
        /// <param name="valuepair">data to be read from</param>
        /// <returns>Sets the Usage property of the Viewmodel</returns>
        public async Task<bool> GetUsage(KeyValuePair[] valuepair)
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

            var loop = false;
            var list = valuepair.ToList();
            for (var i = 1; !loop; i++)
            {
                if (list.Any(x => x.name == VikingApi.Json.Usage.Page))
                    list.RemoveAt(list.Count - 1);
                list.Add(new KeyValuePair { content = i, name = VikingApi.Json.Usage.Page });
                var json = await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Usage, list.ToArray());

                if (string.Equals(json, "[]"))
                    loop = true;
                if (string.IsNullOrEmpty(json) && i == 1)
                    return false;

                Usage = (i == 1) ? JsonConvert.DeserializeObject<Usage[]>(json) : Usage.Concat(JsonConvert.DeserializeObject<Usage[]>(json));
            }
            Tools.Tools.SetProgressIndicator(false);
            return true;
        }
    }
}
