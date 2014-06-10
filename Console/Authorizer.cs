using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AsyncOAuth;

namespace Console
{
    class Authorizer
    {
        private const string BaseUrl = "https://mobilevikings.com:443/api/2.0/oauth/";

        private const string AuthorizeTokenUrl = BaseUrl + "authorize/";

        public Authorizer()
        {
            
            /*var myUri = new Uri(AuthorizeTokenUrl);
            var myRequest = (HttpWebRequest)WebRequest.Create(myUri);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            using (var postStream = myRequest.EndGetRequestStream(myRequest.BeginGetRequestStream(GetRequestStreamCallback, myRequest)))
            {
                // Create the post data
                const string postData = "consumer_key=un9HyMLftXRtDf89jP&consumer_secret=AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
                var byteArray = Encoding.UTF8.GetBytes(postData);

                // Add the post data to the web request
                postStream.Write(byteArray, 0, byteArray.Length);
            }
            myRequest.BeginGetResponse(GetResponsetStreamCallback, myRequest);*/

        }

        public async Task<TokenResponse<T>> GetTokenResponse<T>(string url, OAuthMessageHandler handler, HttpContent postValue, Func<string, string, T> tokenFactory) where T : Token
        {
            var client = new HttpClient(handler);

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
        }
 
        /*void GetRequestStreamCallback(IAsyncResult callbackResult)
        {
                var myRequest = (HttpWebRequest)callbackResult.AsyncState;
                // End the stream request operation
                using (Stream postStream = myRequest.EndGetRequestStream(callbackResult))
                {
                    // Create the post data
                    const string postData = "consumer_key=un9HyMLftXRtDf89jP&redirect_uri=http://www.google.com";
                    var byteArray = Encoding.UTF8.GetBytes(postData);

                    // Add the post data to the web request
                    postStream.Write(byteArray, 0, byteArray.Length);
                }
 
                // Start the web request
                
        }

        void GetResponsetStreamCallback(IAsyncResult callbackResult)
        {
                HttpWebRequest request = (HttpWebRequest)callbackResult.AsyncState;
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(callbackResult);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Debug.WriteLine("Ok");
                }
                else
                {
                    Debug.WriteLine(response.StatusCode);
                }
                using (StreamReader httpWebStreamReader = new StreamReader(response.GetResponseStream()))
                {
                    string result = httpWebStreamReader.ReadToEnd();
                    //For debug: show results
                    Debug.WriteLine(result);
                    string[] data = result.Split('=');
                    var url = AuthorizeTokenUrl + "?request_token=" + data[1] + "&redirect_uri=http://www.google.com";
                    code = data[1];
                }
        }*/
    }
}
