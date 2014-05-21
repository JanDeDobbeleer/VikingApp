using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsyncOAuth;
using Fuel.Localization.Resources;
using Microsoft.Phone.Shell;
using Microsoft.Phone.UserData;
using Newtonsoft.Json;
using Tools;
using VikingApi.Api;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Migrate
{
    public class UsageViewmodel : CancelAsyncTask
    {
        public string Msisdn { get; set; }
        public IEnumerable<Usage> Usage { get; private set; }
        public IEnumerable<Contact> Contacts { get; private set; }
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

        public UsageViewmodel()
        {
            var contacts = new Contacts();
            contacts.SearchCompleted += contacts_SearchCompleted;
            contacts.SearchAsync(string.Empty, FilterKind.None, "Contact test");
        }

        void contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            Contacts = e.Results;
        }

        public async Task<bool> GetUsage(DateTime fromDate, DateTime untilDate, int page = 1)
        {
            if (page == 1)
            {
                _page = page;
                _date1 = fromDate;
                _date2 = untilDate;
                Usage = null;
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
            SystemTray.ProgressIndicator.Text = AppResources.ProgressRetrievingUsage;
            var client = new VikingsApi();
            client.GetInfoFinished += client_GetInfoFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenKey.ToString()], (string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenSecret.ToString()]), client.Usage, pair, Cts);
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
                        try
                        {
                            Usage = (_page == 1) ? JsonConvert.DeserializeObject<Usage[]>(args.Json) : Usage.Concat(JsonConvert.DeserializeObject<Usage[]>(args.Json));
                            await GetUsage(_date1, _date2, ++_page);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                        return;
                    }
                    Tools.Tools.SetProgressIndicator(false);
                    break;
            }
            OnGetInfoFinished(args);
        }
    }
}
