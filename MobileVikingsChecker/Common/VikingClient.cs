using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncOAuth;
using Microsoft.Phone.Net.NetworkInformation;

namespace MobileVikingsChecker.Common
{
    class VikingsClient
    {
        private readonly string _consumerKey;
        private readonly string _consumerSecret;
        private readonly AccessToken _accessToken;
        private static RequestToken _requestToken;
        private static OAuthAuthorizer _authorizer;

        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string RequestTokenUrl = BaseUrl + "request_token/";
        private const string AuthorizeTokenUrl = BaseUrl + "authorize/";
        private const string AccessTokenUrl = BaseUrl + "access_token/";

        public VikingsClient(string consumerKey, string consumerSecret, AccessToken accessToken)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _accessToken = accessToken;
        }

        // sample flow for Twitter authroize
        public async static Task<string> GetPinRequestUrl(string consumerKey, string consumerSecret)
        {
            // create authorizer
            _authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get request token
            var tokenResponse = await _authorizer.GetRequestToken(
                RequestTokenUrl,
                new[] { new KeyValuePair<string, string>("oauth_callback", "oob") });
            _requestToken = tokenResponse.Token;

            return _authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, _requestToken);
        }

        public async static Task<AccessToken> Authorize(string pinCode)
        {
            // get access token
            var accessTokenResponse = await _authorizer.GetAccessToken(AccessTokenUrl, _requestToken, pinCode);

            // save access token.
            var accessToken = accessTokenResponse.Token;

            return accessToken;
        }

        public async Task<string> GetBalance()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "sim_balance.json");
            return json;
        }

        public async Task<string> GetSims()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "msisdn_list.json?alias=1");
            return json;
        }

        public async Task<string> GetPricePlan()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetTopUpHistory()
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //TODO: write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + string.Format("top_up_history.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetUsage()
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + string.Format("usage.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetSimInfo()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetVikingPointsStats()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "points/stats.json");
            return json;
        }

        public async Task<string> GetVikingPointsReferalLinks()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "points/links.json");
            return json;
        }

        //REFERRALS
        public async Task<string> GetVikingPointsReferrals()
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, _accessToken);

            var json = await client.GetStringAsync(BaseUrl + "points/referrals.json");
            return json;
        }

    }
}
