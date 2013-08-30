using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using AsyncOAuth;

namespace Console
{
    internal class VikingsClient
    {
        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";
        private const string RequestTokenUrl = BaseUrl + "request_token/";
        private const string AuthorizeTokenUrl = BaseUrl + "authorize/";
        private const string AccessTokenUrl = BaseUrl + "access_token/";
        private readonly string consumerKey;
        private readonly string consumerSecret;
        public AccessToken accessToken;

        public VikingsClient(string consumerKey, string consumerSecret, AccessToken accessToken)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
        }

        // sample flow for Twitter authroize
        public static async Task<AccessToken> Authorize(string consumerKey, string consumerSecret)
        {
            // create authorizer
            var authorizer = new OAuthAuthorizer(consumerKey, consumerSecret);

            // get request token
            TokenResponse<RequestToken> tokenResponse = await authorizer.GetRequestToken(
                RequestTokenUrl,
                new[] {new KeyValuePair<string, string>("oauth_callback", "oob")});
            RequestToken requestToken = tokenResponse.Token;

            string pinRequestUrl = authorizer.BuildAuthorizeUrl(AuthorizeTokenUrl, requestToken);

            // open browser and get PIN Code
            Process.Start(pinRequestUrl);

            // enter pin
            System.Console.WriteLine("ENTER PIN");
            string pinCode = System.Console.ReadLine();

            // get access token
            TokenResponse<AccessToken> accessTokenResponse = await authorizer.GetAccessToken(AccessTokenUrl, requestToken, pinCode);

            // save access token.
            AccessToken accessToken = accessTokenResponse.Token;

            return accessToken;
        }

        public async Task<string> GetBalance()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "sim_balance.json");
            return json;
        }

        public async Task<string> GetSims()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "msisdn_list.json?alias=1");
            return json;
        }

        public async Task<string> GetPricePlan()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetTopUpHistory()
        {
            DateTime fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //write extension to convert time to API format
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + string.Format("top_up_history.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetUsage()
        {
            DateTime fromdate = DateTime.Now.AddMonths(-1);
            //API requires: YYYY-MM-DDTHH:MM:SS
            //write extension to convert time to API format
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + string.Format("usage.json?fromdate={0}", fromdate.ToString("yyyy-MM-ddTHH:mm:ss")));
            return json;
        }

        public async Task<string> GetSimInfo()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "price_plan_details.json");
            return json;
        }

        public async Task<string> GetVikingPointsStats()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "points/stats.json");
            return json;
        }

        public async Task<string> GetVikingPointsReferalLinks()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "points/links.json");
            return json;
        }

        //REFERRALS
        public async Task<string> GetVikingPointsReferrals()
        {
            HttpClient client = OAuthUtility.CreateOAuthClient(consumerKey, consumerSecret, accessToken);

            string json = await client.GetStringAsync(BaseUrl + "points/referrals.json");
            return json;
        }
    }
}