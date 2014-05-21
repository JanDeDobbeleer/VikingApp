using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tools;

namespace VikingApi.Api
{
    public class Xauth : OAuthBase
    {
        private const string ConsumerKey = "un9HyMLftXRtDf89jP";
        private const string ConsumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";
        public const string XauthAccessToken = "https://mobilevikings.com:443/api/2.0/oauth/access_token/";

        public async Task<bool> XAuthAccessTokenGet(string username, string password)
        {
            try
            {
                string outUrl;
                string querystring;
                var nonce = GenerateNonce();
                var timeStamp = GenerateTimeStamp();
                var sig = GenerateSignature(new Uri(XauthAccessToken),
                    ConsumerKey,
                    ConsumerSecret,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    username,
                    password,
                    "GET",
                    timeStamp,
                    nonce,
                    out outUrl,
                    out querystring);

                querystring += "&oauth_signature=" + HttpUtility.UrlEncode(sig);

                if (querystring.Length > 0)
                    outUrl += "?";

                var webRequest = WebRequest.Create(outUrl + querystring) as HttpWebRequest;
                if (webRequest == null)
                    throw new Exception("Can't create webrequest");

                webRequest.Method = "GET";
                var response = await webRequest.GetResponseAsync();
                StreamReader responseReader = null;
                string responseData;
                try
                {
                    responseReader = new StreamReader(response.GetResponseStream());
                    responseData = responseReader.ReadToEnd();
                }
                finally
                {
                    response.GetResponseStream().Close();
                    if (responseReader != null)
                        responseReader.Close();
                }
                if (responseData.Length > 0)
                {
                    //Store the Token and Token Secret
                    var qs = GetQueryParameters(responseData);
                    if (qs["oauth_token"] != null && qs["oauth_token_secret"] != null)
                    {
                        Settings.GetInstance().SaveSetting(new[]
                        {
                            new KeyValuePair {Name = Setting.Login.ToString(), Content = false},
                            new KeyValuePair {Name = Setting.TokenKey.ToString(), Content = qs["oauth_token"]},
                            new KeyValuePair {Name = Setting.TokenSecret.ToString(), Content = qs["oauth_token_secret"]}
                        });
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private Dictionary<string, string> GetQueryParameters(string response)
        {
            var nameValueCollection = new Dictionary<string, string>();
            var items = response.Split('&');

            foreach (var item in items)
            {
                if (!item.Contains("=")) 
                    continue;
                var nameValue = item.Split('=');
                if (nameValue[0].Contains("?"))
                    nameValue[0] = nameValue[0].Replace("?", "");
                nameValueCollection.Add(nameValue[0], HttpUtility.UrlDecode(nameValue[1]));
            }
            return nameValueCollection;
        }
    }
}
