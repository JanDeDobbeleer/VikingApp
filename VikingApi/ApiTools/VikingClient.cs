using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncOAuth;
using Tools;

namespace VikingApi.ApiTools
{
    public class VikingsClient
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
        private const string Parameter = "?{0}={1}";
        private const string _balance = "sim_balance.json";
        public const string _sim = "msisdn_list.json";
        private static RequestToken _requestToken;
        private static OAuthAuthorizer _authorizer;

        //public properties
        public string Balance
        {
            get { return _balance; }
        }

        public string Sim
        {
            get { return _sim; }
        }

        public static async Task<string> GetPinUrl()
        {
            if (!ApiTools.HasInternetConnection()) return null;
            // create authorizer
            _authorizer = new OAuthAuthorizer(ConsumerKey, ConsumerSecret);

            // get request token
            TokenResponse<RequestToken> tokenResponse = await _authorizer.GetRequestToken(
                RequestTokenUrl,
                new[] {new KeyValuePair<string, string>("oauth_callback", "oob")});
            _requestToken = tokenResponse.Token;

            //Get and return Pin URL
            return _authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, _requestToken);
        }

        public static async Task<AccessToken> GetAccessToken(string pinCode)
        {
            if (!ApiTools.HasInternetConnection()) return null;
            // get access token
            TokenResponse<AccessToken> accessTokenResponse = await _authorizer.GetAccessToken(AccessTokenUrl, _requestToken, pinCode);

            // save access token.
            AccessToken accessToken = accessTokenResponse.Token;

            return accessToken;
        }

        public async Task<string> GetInfo(AccessToken token, string path)
        {
            if (!ApiTools.HasInternetConnection()) return null;
            try
            {
                HttpClient client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);
                string json = await client.GetStringAsync(BaseUrl + path);
                return json;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> GetInfo(AccessToken token, string path, KeyValuePair valuePair)
        {
            if (valuePair.content != null && valuePair.name != null)
                return await GetInfo(token, path + string.Format(Parameter, valuePair.name, HttpUtility.UrlEncode((string) valuePair.content)));
            return null;
        }

        /*public async Task<string> GetBalance(AccessToken token)
        {
            if (!ApiTools.HasInternetConnection()) return null;
            try
            {
                var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

                var json = await client.GetStringAsync(BaseUrl + "sim_balance.json");
                return json;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> GetSims(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "msisdn_list.json?alias=1");
            return json;
        }

        public async Task<string> GetPricePlan(AccessToken token)
        {
            try
            {
                var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

                var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
                return json;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<string> GetTopUpHistory(AccessToken token)
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //TODO: write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + string.Format("top_up_history.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetUsage(AccessToken token)
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + string.Format("usage.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetSimInfo(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetVikingPointsStats(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/stats.json");
            return json;
        }

        public async Task<string> GetVikingPointsReferalLinks(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/links.json");
            return json;
        }

        //REFERRALS
        public async Task<string> GetVikingPointsReferrals(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(ConsumerKey, ConsumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/referrals.json");
            return json;
        }*/
    }
}