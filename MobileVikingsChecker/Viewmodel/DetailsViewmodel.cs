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
    public class DetailsViewmodel: CancelAsyncTask
    {
        public string Msisdn;
        public IEnumerable<Usage> Usage;

        #region event handling
        public event GetInfoFinishedEventHandler GetInfoFinished;

        protected void OnGetInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetInfoFinished != null)
            {
                GetInfoFinished(this, args);
            }
        }
        #endregion

        public async Task<bool> GetUsage(DateTime fromDate, DateTime untilDate)
        {
            var pair = new[]
            {
                new KeyValuePair{content = Msisdn, name = VikingApi.Json.Usage.Msisdn},
                new KeyValuePair{content = fromDate.ToVikingApiTimeFormat(), name = VikingApi.Json.Usage.FromDate},
                new KeyValuePair{content = untilDate.ToVikingApiTimeFormat(), name = VikingApi.Json.Usage.UntilDate},
                new KeyValuePair{content = "100", name = VikingApi.Json.Usage.PageSize} 
            };
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "retrieving information";
            var client = new VikingsApi();
            client.GetInfoFinished += client_GetInfoFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Usage, pair, Cts);
            return true;
        }

        private void client_GetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            switch (args.Canceled)
            {
                case true:
                    Tools.Tools.SetProgressIndicator(false);
                    break;
                case false:
                    if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                        return;
                    Usage = JsonConvert.DeserializeObject<Usage[]>(args.Json);
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            OnGetInfoFinished(args);
        }
    }
}
