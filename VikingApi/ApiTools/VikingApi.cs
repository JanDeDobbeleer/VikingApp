using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncOAuth;
using Tools;

namespace VikingApi.ApiTools
{
    public delegate void GetInfoFinishedEventHandler(object sender, GetInfoCompletedArgs args);

    public class VikingsApi:IDisposable
    {
        //data to authenticate
        private const string ConsumerKey = "un9HyMLftXRtDf89jP";
        private const string ConsumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";

        //base url's for authentication
        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string RequestTokenUrl = BaseUrl + "request_token/";
        private const string AuthorizeTokenUrl = BaseUrl + "authorize/";
        private const string AccessTokenUrl = BaseUrl + "access_token/";

        //constant to build paths
        private const string Parameter = "{0}={1}";
        private const string _balance = "sim_balance.json";
        private const string _sim = "msisdn_list.json";
        private const string _usage = "usage.json";
        private static RequestToken _requestToken;
        private static OAuthAuthorizer _authorizer;

        private IntPtr _nativeResource = Marshal.AllocHGlobal(100);

        //public properties
        public string Balance
        {
            get { return _balance; }
        }

        public string Sim
        {
            get { return _sim; }
        }

        public string Usage
        {
            get { return _usage; }
        }

        //event handling

        public event GetInfoFinishedEventHandler GetInfoFinished;

        protected void OnGetInfoFinished(GetInfoCompletedArgs args)
        {
            if (GetInfoFinished != null)
            {
                GetInfoFinished(this, args);
            }
        }

        public static async Task<string> GetPinUrl()
        {
            if (!ApiTools.HasInternetConnection()) return null;
            // create authorizer
            _authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);

            // get request token
            var tokenResponse = await _authorizer.GetRequestToken(
                RequestTokenUrl,
                new[] { new KeyValuePair<string, string>("oauth_callback", "oob") });
            _requestToken = tokenResponse.Token;

            //Get and return Pin URL
            return _authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, _requestToken);
        }

        public static async Task<AccessToken> GetAccessToken(string pinCode)
        {
            if (!ApiTools.HasInternetConnection()) return null;
            // get access token
            var accessTokenResponse = await _authorizer.GetAccessToken(AccessTokenUrl, _requestToken, pinCode);

            // save access token.
            var accessToken = accessTokenResponse.Token;

            return accessToken;
        }

        public async Task<bool> GetInfo(AccessToken token, string path, CancellationTokenSource cts)
        {
            var args = new GetInfoCompletedArgs();
            if (!ApiTools.HasInternetConnection())
            {
                Message.ShowToast("Please connect to the internet and try again.");
                return false;
            }
            try
            {
                using (var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token))
                {
                    if (!cts.Token.IsCancellationRequested)
                    {
                        using (cts.Token.Register(() => client.CancelPendingRequests()))
                        {
                            args.Json = await client.GetStringAsync(BaseUrl + path);
                            args.Canceled = false;
                        }
                    }
                }
                OnGetInfoFinished(args);
            }
            catch (HttpRequestException e)
            {
                Message.ShowToast(e.Message.ToErrorMessage());
                args.Canceled = true;
                OnGetInfoFinished(args);
            }
            catch (TaskCanceledException)
            {
                args.Canceled = true;
                OnGetInfoFinished(args);
            }
            catch (Exception)
            {
                args.Canceled = true;
                OnGetInfoFinished(args);
                //TODO: send mail to me with error
            }
            return true;
        }

        public async Task<bool> GetInfo(AccessToken token, string path, KeyValuePair valuePair, CancellationTokenSource cts)
        {
            if (valuePair.content != null && valuePair.name != null)
                return await GetInfo(token, path + "?" + string.Format(Parameter, valuePair.name, HttpUtility.UrlEncode((string)valuePair.content)), cts);
            return false;
        }

        public async Task<bool> GetInfo(AccessToken token, string path, KeyValuePair[] valuePair, CancellationTokenSource cts)
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("?{0}={1}", valuePair[0].name, HttpUtility.UrlEncode(((string)valuePair[0].content))));
            for (var i = 1; i < valuePair.Count(); i++)
            {
                builder.Append(string.Format("&{0}={1}", valuePair[i].name, valuePair[i].content));
            }
            return await GetInfo(token, path + builder, cts);
        }

        #region IDisposable
        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~VikingsApi()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            // free native resources if there are any.
            if (_nativeResource == IntPtr.Zero)
                return;
            Marshal.FreeHGlobal(_nativeResource);
            _nativeResource = IntPtr.Zero;
        }
        #endregion
    }
}