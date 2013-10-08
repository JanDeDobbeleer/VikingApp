using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Net.NetworkInformation;

namespace UpdateTile
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

        public async Task<bool> GetInfo(AccessToken token, string path)
        {
            var args = new GetInfoCompletedArgs();
            if (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None)
            {

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
            return false;
        }

        public async Task<bool> GetInfo(AccessToken token, KeyValuePair valuePair)
        {
            if (valuePair.content != null && valuePair.name != null)
                return await GetInfo(token, "sim_balance.json" + "?" + string.Format(Parameter, valuePair.name, HttpUtility.UrlEncode((string)valuePair.content)));
            return false;
        }
    }
}
