using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AsyncOAuth;
using Fuel.Localization.Resources;
using Microsoft.Phone.Shell;
using Newtonsoft.Json;
using Tools;
using VikingApi.Api;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.Migrate
{
    public class SimDetailsViewmodel : CancelAsyncTask
    {
        public string Msisdn { get; set; }
        public IEnumerable<TopUp> Topup { get; private set; }
        public PricePlan Plan { get; private set; }
        public SimCard Card { get; private set; }
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

        public event GetInfoFinishedEventHandler GetPlanInfoFinished;

        protected void OnGetPlanInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetPlanInfoFinished != null)
            {
                GetPlanInfoFinished(this, args);
            }
        }

        public event GetInfoFinishedEventHandler GetSimInfoFinished;

        protected void OnGetSimInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetSimInfoFinished != null)
            {
                GetSimInfoFinished(this, args);
            }
        }
        #endregion

        public async Task<bool> GetTopUps(DateTime fromDate, DateTime untilDate, int page = 1)
        {
            if (page == 1)
            {
                _page = page;
                _date1 = fromDate;
                _date2 = untilDate;
            }
            var pair = new[]
            {
                new KeyValuePair{Content = Msisdn, Name = "msisdn"},
                new KeyValuePair{Content = fromDate.ToVikingApiTimeFormat(), Name = "from_date"},
                new KeyValuePair{Content = untilDate.ToVikingApiTimeFormat(), Name = "until_date"},
                new KeyValuePair{Content = "100", Name = "page_size"},
                new KeyValuePair{Content = page, Name = "page"}
            };
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.ProgressRetrievingTopups;
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetInfoFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenKey.ToString()], (string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenSecret.ToString()]), client.TopUp, pair, Cts);
            }
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
                            Topup = (_page == 1) ? JsonConvert.DeserializeObject<TopUp[]>(args.Json) : Topup.Concat(JsonConvert.DeserializeObject<TopUp[]>(args.Json));
                            await GetTopUps(_date1, _date2, ++_page);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                        return;
                    }
                    break;
            }
            OnGetInfoFinished(args);
        }

        public async Task<bool> GetPlan()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.ProgressRetrievingPricePlanInfo;
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetPlanInfoFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenKey.ToString()], (string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenSecret.ToString()]), client.PricePlan, new KeyValuePair { Content = Msisdn, Name = "msisdn" }, Cts);
            }
            return true;
        }

        private void client_GetPlanInfoFinished(object sender, GetInfoCompletedArgs args)
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
                            Plan = JsonConvert.DeserializeObject<PricePlan>(args.Json);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                    }
                    break;
            }
            OnGetPlanInfoFinished(args);
        }

        public async Task<bool> GetSimInfo()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.ProgressRetrievingCardInfo;
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetSimInfoFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenKey.ToString()], (string)IsolatedStorageSettings.ApplicationSettings[Setting.TokenSecret.ToString()]), client.Card, new KeyValuePair { Content = Msisdn, Name = "msisdn" }, Cts);
            }
            return true;
        }

        private void client_GetSimInfoFinished(object sender, GetInfoCompletedArgs args)
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
                            Card = JsonConvert.DeserializeObject<SimCard>(args.Json);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                    }
                    break;
            }
            OnGetSimInfoFinished(args);
        }
    }
}
