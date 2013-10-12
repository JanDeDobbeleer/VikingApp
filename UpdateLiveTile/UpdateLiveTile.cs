using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AsyncOAuth;
using Microsoft.Phone.Shell;
using UpdateLiveTile.Classes;
using UpdateLiveTile.Control;

namespace UpdateLiveTile
{
    public class UpdateLiveTile
    {
        private readonly UserBalance _balance = new UserBalance();
        private readonly Api _client = new Api();
        const string Filename = "/Shared/ShellContent/CustomTile.jpg";

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
            if (string.IsNullOrEmpty((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
                return;
            await GetData((string)IsolatedStorageSettings.ApplicationSettings["sim"]);
#if(DEBUG)
            Debug.WriteLine("Live tile: Start updating Live tile for number " + (string)IsolatedStorageSettings.ApplicationSettings["sim"]);
#endif

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
            {
                var newTile = new FlipTileData
                {
                    Count = 0,
                    BackContent = "unavailable",
                    BackBackgroundImage = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] ? new Uri("/Assets/336x336empty.png", UriKind.Relative) : new Uri("/Assets/336x336redempty.png", UriKind.Relative)
                };
                var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
                if (firstOrDefault != null)
                    firstOrDefault.Update(newTile);
                return;
            }
            if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                return;
            _balance.Load(args.Json);
            SetTile();
#if(DEBUG)
            Debug.WriteLine("Live tile: Tile has been updated");
#endif
        }

        private void SetTile()
        {
            SaveImage();
            var newTile = new FlipTileData
            {
                Count = _balance.Remaining,
                BackBackgroundImage = new Uri("isostore:" + Filename, UriKind.Absolute),
                BackContent = null,
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }

        /*private string BuildInfoString()
        {
            var info = _balance.Credit + Environment.NewLine;
            var data = _balance.Data ?? "0";
            info += data + " MB" + Environment.NewLine;
            var sms = _balance.Sms ?? "0"; 
            info += sms + " SMS" + Environment.NewLine;
#if(DEBUG)
            Debug.WriteLine("Live tile: Back content = " + info);
#endif
            return info;
        }*/

        public void SaveImage()
        {
            var color = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"] : (SolidColorBrush)Application.Current.Resources["VikingColorBrush"];
            var data = _balance.Data ?? "0";
            data += " MB";
            var sms = _balance.Sms ?? "0"; 
            sms += " SMS";
            var vSms = _balance.VikingSms ?? "0";
            vSms += " vSMS";
            var vCall = _balance.VikingMinutes ?? "0"; 
            vCall += " vCall";
            var customTile = new Tile(color,_balance.Credit, data, sms, vSms, vCall);
            customTile.Measure(new Size(336, 336));
            customTile.Arrange(new Rect(0, 0, 336, 336));

            var bmp = new WriteableBitmap(336, 336);
            bmp.Render(customTile, null);
            bmp.Invalidate();

            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.DirectoryExists("/CustomLiveTiles"))
                {
                    isf.CreateDirectory("/CustomLiveTiles");
                }

                using (var stream = isf.OpenFile(Filename, System.IO.FileMode.OpenOrCreate))
                {
                    bmp.SaveJpeg(stream, 336, 366, 0, 100);
                }
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: Image created");
#endif
        }
    }
}
