using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private const string _top_up_history = "top_up_history.json";
        private const string _price_plan = "price_plan_details.json";
        private const string _sim_info = "sim_info.json";
        private const string _stats = "points/stats.json";
        private const string _links = "points/links.json";
        private const string _referrals = "points/referrals.json";
        private static RequestToken _requestToken;
        private static OAuthAuthorizer _authorizer;

        private IntPtr _nativeResource = Marshal.AllocHGlobal(100);

        #region Uri
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

        public string TopUp
        {
            get { return _top_up_history; }
        }

        public string PricePlan
        {
            get { return _price_plan; }
        }

        public string Card
        {
            get { return _sim_info; }
        }

        public string Stats
        {
            get { return _stats; }
        }

        public string Links
        {
            get { return _links; }
        }

        public string Referrals
        {
            get { return _referrals; }
        }
        #endregion

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

        public async Task<string> GetPinUrl()
        {
            try
            {
                if (!ApiTools.HasInternetConnection()) return null;
                // create authorizer
                _authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);

                // get request token
                var tokenResponse = await _authorizer.GetRequestToken(
                    RequestTokenUrl,
                    new[] { new KeyValuePair<string, string>("oauth_callback", "oob") });
                /*var tokenResponse = await GetRequestToken(
                    RequestTokenUrl,
                    new[] { new KeyValuePair<string, string>("oauth_callback", "oob"), new KeyValuePair<String, String>("debug", "true") });*/

                _requestToken = tokenResponse.Token;

                //Get and return Pin URL
                return _authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, _requestToken);
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<AccessToken> GetAccessToken(string pinCode)
        {
            try
            {
                if (!ApiTools.HasInternetConnection()) return null;
                // get access token
                _authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);
                var accessTokenResponse = await _authorizer.GetAccessToken(AccessTokenUrl, _requestToken, pinCode);

                // save access token.
                var accessToken = accessTokenResponse.Token;
                return accessToken;
            }
            catch (HttpRequestException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> GetInfo(AccessToken token, string path, CancellationTokenSource cts)
        {
            var args = new GetInfoCompletedArgs();
            if (!ApiTools.HasInternetConnection())
            {
                Message.ShowToast("Bummer, it looks like we're all out of internet.");
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
                return true;
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
            }
            return true;
        }

        public async Task<bool> GetInfo(AccessToken token, string path, KeyValuePair valuePair, CancellationTokenSource cts)
        {
            if (valuePair.Content != null && valuePair.Name != null)
                return await GetInfo(token, path + "?" + string.Format(Parameter, valuePair.Name, HttpUtility.UrlEncode((string)valuePair.Content)), cts);
            return false;
        }

        public async Task<bool> GetInfo(AccessToken token, string path, KeyValuePair[] valuePair, CancellationTokenSource cts)
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("?{0}={1}", valuePair[0].Name, HttpUtility.UrlEncode(((string)valuePair[0].Content))));
            for (var i = 1; i < valuePair.Count(); i++)
            {
                builder.Append(string.Format("&{0}={1}", valuePair[i].Name, valuePair[i].Content));
            }
            return await GetInfo(token, path + builder, cts);
        }

        /*public async Task<TokenResponse<T>> GetTokenResponse<T>(string url, OAuthMessageHandler handler, HttpContent postValue, Func<string, string, T> tokenFactory) where T : Token
        {
            var client = new HttpClient(handler);
            //try to change the headers
            client.DefaultRequestHeaders.Referrer = new Uri(AuthorizeTokenUrl, UriKind.RelativeOrAbsolute);
            //client.DefaultRequestHeaders.Connection.Add("keep-alive");
            client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue() { MaxAge = TimeSpan.Zero };
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,#1#*;q=0.8");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip,deflate,sdch");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Charset", "ISO-8859-1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/29.0.1547.76 Safari/537.36");

            var response = await client.PostAsync(AuthorizeTokenUrl, postValue ?? new FormUrlEncodedContent(Enumerable.Empty<KeyValuePair<string, string>>())).ConfigureAwait(false);
            var tokenBase = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpRequestException(response.StatusCode + ":" + tokenBase); // error message
            }

            var splitted = tokenBase.Split('&').Select(s => s.Split('=')).ToLookup(xs => xs[0], xs => xs[1]);
            var token = tokenFactory(splitted["oauth_token"].First().UrlDecode(), splitted["oauth_token_secret"].First().UrlDecode());
            var extraData = splitted.Where(kvp => kvp.Key != "oauth_token" && kvp.Key != "oauth_token_secret")
                .SelectMany(g => g, (g, value) => new { g.Key, Value = value })
                .ToLookup(kvp => kvp.Key, kvp => kvp.Value);
            return new TokenResponse<T>(token, extraData);
        }

        public Task<TokenResponse<RequestToken>> GetRequestToken(string requestTokenUrl, IEnumerable<KeyValuePair<string, string>> parameters = null, HttpContent postValue = null)
        {
            //Precondition.NotNull(requestTokenUrl, "requestTokenUrl");

            var handler = new OAuthMessageHandler("un9HyMLftXRtDf89jP", "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6", token: null, optionalOAuthHeaderParameters: parameters);
            return GetTokenResponse(requestTokenUrl, handler, postValue, (key, secret) => new RequestToken(key, secret));
        }*/

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
            _authorizer = null;
            Marshal.FreeHGlobal(_nativeResource);
            _nativeResource = IntPtr.Zero;
        }
        #endregion
    }
}