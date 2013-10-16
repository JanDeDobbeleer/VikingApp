﻿using System;
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
    public class SimDetailsViewmodel : CancelAsyncTask
    {
        public string Msisdn;
        public IEnumerable<TopUp> Topup;
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
                new KeyValuePair{Content = Msisdn, Name = "msisdn"},
                new KeyValuePair{Content = fromDate.ToVikingApiTimeFormat(), Name = "from_date"},
                new KeyValuePair{Content = untilDate.ToVikingApiTimeFormat(), Name = "until_date"},
                new KeyValuePair{Content = "100", Name = "page_size"},
                new KeyValuePair{Content = page, Name = "page"},
            };
            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = "retrieving information";
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
                await client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), client.TopUp, pair, Cts);
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
                        Topup = (_page == 1) ? JsonConvert.DeserializeObject<TopUp[]>(args.Json) : Topup.Concat(JsonConvert.DeserializeObject<TopUp[]>(args.Json));
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