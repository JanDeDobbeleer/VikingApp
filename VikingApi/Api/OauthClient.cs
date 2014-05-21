using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VikingApi.Api
{
    public struct OAuthToken
    {
        public String Token;
        public String Secret;
    }

    public class OAuthClient: OAuthBase
    {
        public String ConsumerKey = String.Empty;
        public String ConsumerSecret = String.Empty;
        public OAuthToken Token = new OAuthToken { Token = String.Empty, Secret = String.Empty };
        protected OAuthBase OAuthBase;
        

        public OAuthClient(String consumerKey, String consumerSecret)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            OAuthBase = new OAuthBase();
        }


        protected async Task<string> RawRequest(String method, Uri uri, List<KeyValuePair<string, string>> data)
        {
            var nonce = OAuthBase.GenerateNonce();
            var timeStamp = OAuthBase.GenerateTimeStamp();
            string parameters;
            string normalizedUrl;
            var signingUri = new Uri(uri.ToString());

            var signature = OAuthBase.GenerateSignature(signingUri, ConsumerKey, ConsumerSecret,
                Token.Token, Token.Secret, method, timeStamp, nonce, SignatureTypes.HMACSHA1,
                out normalizedUrl, out parameters);

            signature = HttpUtility.UrlEncode(signature);

            var requestUri = new StringBuilder(uri.ToString());
            requestUri.Append(uri.ToString().Contains("?") ? "&" : "?");
            requestUri.AppendFormat("oauth_consumer_key={0}&", ConsumerKey);
            requestUri.AppendFormat("oauth_nonce={0}&", nonce);
            requestUri.AppendFormat("oauth_timestamp={0}&", timeStamp);
            requestUri.AppendFormat("oauth_signature_method={0}&", "HMAC-SHA1");
            requestUri.AppendFormat("oauth_version={0}&", "1.0");
            requestUri.AppendFormat("oauth_signature={0}", signature);
            if (Token.Token != String.Empty)
                requestUri.AppendFormat("&oauth_token={0}", Token.Token);

            var webRequest = WebRequest.Create(requestUri.ToString()) as HttpWebRequest;
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
            return responseData;
        }

        public async Task<string> Request(String method, Uri uri, List<KeyValuePair<string, string>> data)
        {
            return await RawRequest(method, uri, data);
        }
    }
}
