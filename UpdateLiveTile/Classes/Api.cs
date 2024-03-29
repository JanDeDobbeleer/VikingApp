﻿using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Net.NetworkInformation;

namespace UpdateLiveTile.Classes
{
    public struct KeyValuePair
    {
        public object content;
        public string name;
    }

    public delegate void GetInfoFinishedEventHandler(object sender, GetInfoCompletedArgs args);

    class Api
    {
        public event GetInfoFinishedEventHandler GetInfoFinished;
        private const string ConsumerKey = "un9HyMLftXRtDf89jP";
        private const string ConsumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string Parameter = "{0}={1}";

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

        public async Task<bool> GetInfo(AccessToken token, string path, bool fromForeground)
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
                            if (fromForeground)
                            {
                                args.Json = await client.GetStringAsync(BaseUrl + path);
                            }
                            else
                            {
                                var info = client.GetStreamAsync(BaseUrl + path);
                                var reader = new StreamReader(info.Result);
                                args.Json = reader.ReadToEnd();
                            }
                            args.Canceled = false;
                            //args.Canceled = true;
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

        public async Task<bool> GetInfo(AccessToken token, KeyValuePair valuePair, bool fromForeground)
        {
            if (valuePair.content != null && valuePair.name != null)
                return await GetInfo(token, "sim_balance.json" + "?" + string.Format(Parameter, valuePair.name, HttpUtility.UrlEncode((string)valuePair.content)), fromForeground);
            return false;
        }
    }
}
