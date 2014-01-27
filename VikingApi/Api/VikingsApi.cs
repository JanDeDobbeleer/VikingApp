﻿using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Net.NetworkInformation;
using Tools;
using VikingApi.ApiTools;

namespace VikingApi.Api
{
    public delegate void GetInfoFinishedEventHandler(object sender, GetInfoCompletedArgs args);

    public class VikingsApi : IDisposable
    {
        public event GetInfoFinishedEventHandler GetInfoFinished;
        private const string ConsumerKey = "un9HyMLftXRtDf89jP";
        private const string ConsumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string Parameter = "{0}={1}";

        #region apilocations
        public string Sim
        {
            get { return "msisdn_list.json"; }
        }

        public string Balance
        {
            get { return "sim_balance.json"; }
        }

        public string Usage
        {
            get { return "usage.json"; }
        }
        
        public string Referrals
        {
            get { return "referrals.json"; }
        }

        public string TopUp
        {
            get { return "top_up_history.json"; }
        }

        public string Stats
        {
            get { return "stats.json"; }
        }

        public string Card
        {
            get { return "sim_info.json"; }
        }

        public string Links
        {
            get { return "links.json"; }
        }

        public string PricePlan
        {
            get { return "price_plan_details.json"; }
        }
        #endregion

        protected CancellationTokenSource Cts = new CancellationTokenSource();

        public void CancelTask()
        {
            Cts.Cancel();
        }

        public void RenewToken()
        {
            Cts = new CancellationTokenSource();
        }

        protected void OnGetInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetInfoFinished != null)
            {
                GetInfoFinished(this, args);
            }
        }

        public async Task<bool> Authorize(string userName, string passWord)
        {
            var xAuth = new VikingXauth();
            return await xAuth.XAuthAccessTokenGet(userName, passWord);
        }

        public async Task<bool> GetInfo(AccessToken token, string path, CancellationTokenSource cts)
        {
            var args = new GetInfoCompletedArgs();
            if (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.None)
            {
                args.Canceled = true;
                OnGetInfoFinished(args);
                return true;
            }
            try
            {
                using (var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token))
                {
                    if (!Cts.Token.IsCancellationRequested)
                    {
                        using (Cts.Token.Register(() => client.CancelPendingRequests()))
                        {

                            args.Json = await client.GetStringAsync(BaseUrl + path);
                            args.Canceled = false;
                        }
                        OnGetInfoFinished(args);
                    }
                }
            }
            catch (Exception)
            {
                CancelTask();
                args.Canceled = true;
                OnGetInfoFinished(args);
            }
            return true;
        }

        public async Task<bool> GetInfo(AccessToken token, string location, KeyValuePair valuePair, CancellationTokenSource cts)
        {
            if (valuePair.Content != null && valuePair.Name != null)
                return await GetInfo(token, location + "?" + string.Format(Parameter, valuePair.Name, HttpUtility.UrlEncode((string)valuePair.Content)), cts);
            return false;
        }

        public void Dispose()
        {
            CancelTask();
        }
    }
}
