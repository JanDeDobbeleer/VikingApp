using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using UpdateLiveTile.Classes;

namespace UpdateLiveTile
{
    public class UpdateLiveTile
    {
        private readonly UserBalance _balance = new UserBalance();
        private readonly Api _client = new Api();

        protected CancellationTokenSource Cts = new CancellationTokenSource();

        public void CancelTask()
        {
            Cts.Cancel();
        }

        public void RenewToken()
        {
            Cts = new CancellationTokenSource();
        }

        public async void Start()
        {
            await GetData((string)IsolatedStorageSettings.ApplicationSettings["sim"]);
        }

        private async Task<bool> GetData(string msisdn)
        {
            _client.RenewToken();
            _client.GetInfoFinished += client_GetDataFinished;
            OAuthUtility.ComputeHash = (key, buffer) =>
            {
                using (var hmac = new HMACSHA1(key))
                {
                    return hmac.ComputeHash(buffer);
                }
            };
            await _client.GetInfo(new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"], (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]), new KeyValuePair { name = "msisdn", content = msisdn });
            return true;
        }

        void client_GetDataFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled) 
                return;
            if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                return;
            _balance.Load(args.Json);
            if (_balance.Remaining == 0)
            {
                SetExpiredTile();
            }
            else
            {
                SetTile();
            }
#if(DEBUG)
            Debug.WriteLine("Tile has been updated");
#endif
        }

        private void SetTile()
        {
            var newTile = new FlipTileData
            {
                Count = _balance.Remaining,
                BackContent = BuildInfoString()
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }

        private void SetExpiredTile()
        {
            var newTile = new FlipTileData
            {
                Count = _balance.Remaining,
                BackContent = "expired"
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }

        private string BuildInfoString()
        {
            var info = _balance.Credit + Environment.NewLine;
            info += _balance.Data + " MB" + Environment.NewLine;
            info += _balance.Sms + " SMS" + Environment.NewLine;
            return info;
        }
    }
}
