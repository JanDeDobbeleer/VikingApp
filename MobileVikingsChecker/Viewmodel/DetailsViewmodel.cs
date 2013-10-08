using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using AsyncOAuth;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    public class DetailsViewmodel : CancelAsyncTask
    {
        public string Msisdn;
        public IEnumerable<Usage> Usage;
        private int _page;
        private DateTime _date1;
        private DateTime _date2;


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

        public async Task<bool> GetUsage(DateTime fromDate, DateTime untilDate, int page = 1)
        {
            if (page == 1)
            {
                _page = page;
                _date1 = fromDate;
                _date2 = untilDate;
            }
            var pair = new[]
            {
                new KeyValuePair{Content = Msisdn, Name = VikingApi.Json.Usage.Msisdn},
                new KeyValuePair{Content = fromDate.ToVikingApiTimeFormat(), Name = VikingApi.Json.Usage.FromDate},
                new KeyValuePair{Content = untilDate.ToVikingApiTimeFormat(), Name = VikingApi.Json.Usage.UntilDate},
                new KeyValuePair{Content = "100", Name = VikingApi.Json.Usage.PageSize},
                new KeyValuePair{Content = page, Name = VikingApi.Json.Usage.Page},
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

        private async void client_GetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            switch (args.Canceled)
            {
                case true:
                    Tools.Tools.SetProgressIndicator(false);
                    break;
                case false:
                    if (string.IsNullOrEmpty(args.Json))
                        return;
                    if (!string.Equals(args.Json, "[]"))
                    {
                        Usage = (_page == 1) ? JsonConvert.DeserializeObject<Usage[]>(args.Json) : Usage.Concat(JsonConvert.DeserializeObject<Usage[]>(args.Json));
                        await GetUsage(_date1, _date2, ++_page);
                        return;
                    }
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            OnGetInfoFinished(args);
        }
    }
}
