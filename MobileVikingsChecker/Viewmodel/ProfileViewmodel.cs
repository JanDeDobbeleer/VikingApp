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
using VikingApi.Json;

namespace Fuel.Viewmodel
{
    public class ProfileViewmodel:CancelAsyncTask
    {
        public string Msisdn;
        public IEnumerable<Referral> Referral { get; private set; }
        public IEnumerable<Links> Links { get; private set; } 
        public Stats Stats { get; private set; }
        private int _page;

        #region event handling
        public event GetInfoFinishedEventHandler GetStatsFinished;

        protected void OnGetStatsFinished(GetInfoCompletedArgs args)
        {
            if (GetStatsFinished != null)
            {
                GetStatsFinished(this, args);
            }
        }

        public event GetInfoFinishedEventHandler GetLinkFinished;

        protected void OnGetLinksFinished(GetInfoCompletedArgs args)
        {
            if (GetLinkFinished != null)
            {
                GetLinkFinished(this, args);
            }
        }

        public event GetInfoFinishedEventHandler GetReferralFinished;

        protected void OnGetReferralFinished(GetInfoCompletedArgs args)
        {
            if (GetReferralFinished != null)
            {
                GetReferralFinished(this, args);
            }
        }
        #endregion

        public async Task<bool> GetReferrals(int page = 1)
        {
            if (page == 1)
            {
                _page = page;
            }
            var pair = new[]
            {
                new KeyValuePair{Content = "100", Name = "page_size"},
                new KeyValuePair{Content = page, Name = "page"}
            };
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "retrieving referrals";
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetReferralFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Referrals, pair, Cts);
            }
            return true;
        }

        private async void client_GetReferralFinished(object sender, GetInfoCompletedArgs args)
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
                            Referral = (_page == 1) ? JsonConvert.DeserializeObject<Referral[]>(args.Json) : Referral.Concat(JsonConvert.DeserializeObject<Referral[]>(args.Json));
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                        await GetReferrals(++_page);
                        return;
                    }
                    break;
            }
            OnGetReferralFinished(args);
        }

        public async Task<bool> GetStats()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "getting statistics";
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetStatsFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Stats, Cts);
            }
            return true;
        }

        private void client_GetStatsFinished(object sender, GetInfoCompletedArgs args)
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
                            Stats = JsonConvert.DeserializeObject<Stats>(args.Json);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                    }
                    break;
            }
            OnGetStatsFinished(args);
        }

        public async Task<bool> GetLinks()
        {
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "fetching links";
            using (var client = new VikingsApi())
            {
                client.GetInfoFinished += client_GetLinksFinished;
                OAuthUtility.ComputeHash = (key, buffer) =>
                {
                    using (var hmac = new HMACSHA1(key))
                    {
                        return hmac.ComputeHash(buffer);
                    }
                };
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.Links, Cts);
            }
            return true;
        }

        private void client_GetLinksFinished(object sender, GetInfoCompletedArgs args)
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
                            Links = JsonConvert.DeserializeObject<Links[]>(args.Json);
                        }
                        catch (Exception)
                        {
                            Tools.Tools.SetProgressIndicator(false);
                            return;
                        }
                    }
                    break;
            }
            OnGetLinksFinished(args);
        }
    }
}
