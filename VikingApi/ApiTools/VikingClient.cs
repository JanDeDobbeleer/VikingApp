using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Navigation;
using AsyncOAuth;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace VikingApi.ApiTools
{
    public class VikingsClient
    {

        const string _consumerKey = "un9HyMLftXRtDf89jP";
        const string _consumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
        private static RequestToken _requestToken;
        private static OAuthAuthorizer _authorizer;

        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string RequestTokenUrl = BaseUrl + "request_token/";
        private const string AuthorizeTokenUrl = BaseUrl + "authorize/";
        private const string AccessTokenUrl = BaseUrl + "access_token/";

        public async static Task<string> GetPinUrl()
        {
            if (ApiTools.HasInternetConnection())
            {
                // create authorizer
                _authorizer = new OAuthAuthorizer(_consumerKey, _consumerSecret);

                // get request token
                var tokenResponse = await _authorizer.GetRequestToken(
                    RequestTokenUrl,
                    new[] { new KeyValuePair<string, string>("oauth_callback", "oob") });
                _requestToken = tokenResponse.Token;

                //Get and return Pin URL
                return _authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, _requestToken);
            }
            return null;
        }

        public async static Task<AccessToken> GetAccessToken(string pinCode)
        {
            if (ApiTools.HasInternetConnection())
            {
                // get access token
                var accessTokenResponse = await _authorizer.GetAccessToken(AccessTokenUrl, _requestToken, pinCode);

                // save access token.
                var accessToken = accessTokenResponse.Token;

                return accessToken;
            }
            return null;
        }

        public async Task<string> GetBalance(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "sim_balance.json");
            return json;
        }

        public async Task<string> GetSims(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "msisdn_list.json?alias=1");
            return json;
        }

        public async Task<string> GetPricePlan(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetTopUpHistory(AccessToken token)
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //TODO: write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + string.Format("top_up_history.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetUsage(AccessToken token)
        {
            var fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //write extension to convert time to API format
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + string.Format("usage.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetSimInfo(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetVikingPointsStats(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/stats.json");
            return json;
        }

        public async Task<string> GetVikingPointsReferalLinks(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/links.json");
            return json;
        }

        //REFERRALS
        public async Task<string> GetVikingPointsReferrals(AccessToken token)
        {
            var client = OAuthUtility.CreateOAuthClient(_consumerKey, _consumerSecret, token);

            var json = await client.GetStringAsync(BaseUrl + "points/referrals.json");
            return json;
        }

    }
}
