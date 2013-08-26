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
            /*Run().Wait();
            //System.Console.WriteLine(client.accessToken.Secret);
            GetBalance().Wait();*/

            /*var expires = Convert.ToDateTime("2013-08-25 11:12:55");
            var difference = (expires - DateTime.Now);
            if (int.Parse(difference.TotalDays.ToString().Split('.')[0]) > 0)
            {
                System.Console.WriteLine("Remaining: " + difference.TotalDays.ToString().Split('.')[0] + " days");
            }
            else if(int.Parse(difference.TotalHours.ToString().Split('.')[0]) > 0)
            {
                System.Console.WriteLine("Remaining: " + difference.TotalHours.ToString().Split('.')[0] + " hours");
            }
            else if (int.Parse(difference.TotalMinutes.ToString().Split('.')[0]) > 0)
            {
                System.Console.WriteLine("Remaining: " + difference.TotalMinutes.ToString().Split('.')[0] + " minutes");
            }
            else
            {
                string value = int.Parse(difference.TotalSeconds.ToString().Split('.')[0]) > 0 ? difference.TotalSeconds.ToString().Split('.')[0] : "expired";
                System.Console.WriteLine("Remaining: " + value +" seconds");
            }*/

            /*var try1 = ((Convert.ToDateTime("2013-09-08 07:12:34") - DateTime.Now).TotalDays / (Convert.ToDateTime("2013-09-08 07:12:34").AddMonths(-1) - Convert.ToDateTime("2013-09-08 07:12:34")).Days) * 100d;
            var try2 = (((Convert.ToDateTime("2013-09-08 07:12:34") - Convert.ToDateTime("2013-09-08 07:12:34").AddMonths(-1)).Days - Math.Round((Convert.ToDateTime("2013-09-08 07:12:34") - DateTime.Now).TotalDays, 0)) / (Convert.ToDateTime("2013-09-08 07:12:34") - Convert.ToDateTime("2013-09-08 07:12:34").AddMonths(-1)).Days) * 100d;
            System.Console.WriteLine("total days: " + (Convert.ToDateTime("2013-09-08 07:12:34") - Convert.ToDateTime("2013-09-08 07:12:34").AddMonths(-1)).Days);
            System.Console.WriteLine("passed: " + ((Convert.ToDateTime("2013-09-08 07:12:34") - Convert.ToDateTime("2013-09-08 07:12:34").AddMonths(-1)).Days - Math.Round((Convert.ToDateTime("2013-09-08 07:12:34") - DateTime.Now).TotalDays, 0)));
            System.Console.WriteLine(  try1);
            System.Console.WriteLine();
            System.Console.WriteLine(try2);*/
            var minutes = 3592/60;
            var seconds = 3592 % 60;
            System.Console.WriteLine(string.Format("{0}m {1}s", number, number2));
            System.Console.ReadKey();
        }
    }
}
