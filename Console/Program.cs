using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AsyncOAuth;

namespace Console
{
    class Program
    {
        // set your token
        const string consumerKey = "un9HyMLftXRtDf89jP";
        const string consumerSecret = "AaDM9yyXLmTemvM2nVahzBFYS9JG62a6";

        private static VikingsClient client;
        private static string json;

        static async Task Run()
        {
            // initialize computehash function
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };

            var accessToken = await VikingsClient.Authorize(consumerKey, consumerSecret);
            client = new VikingsClient(consumerKey, consumerSecret, accessToken);
        }

        static async Task GetBalance()
        {
            json = await client.GetBalance();
        }

        static void Main(string[] args)
        {
            Run().Wait();
            //System.Console.WriteLine(client.accessToken.Secret);
            GetBalance().Wait();
            System.Console.WriteLine(json);
            System.Console.ReadKey();
        }
    }
}
