using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
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
        private bool _fromForeground;

        public async void Start(bool fromForeground)
        {
            _fromForeground = fromForeground;
            if (string.IsNullOrEmpty((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
                return;
#if(DEBUG)
            Debug.WriteLine("Live tile: Start updating Live tile for number " + (string)IsolatedStorageSettings.ApplicationSettings["sim"]);
#endif
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
            return await _client.GetInfo(
                        new AccessToken((string)IsolatedStorageSettings.ApplicationSettings["tokenKey"],
                            (string)IsolatedStorageSettings.ApplicationSettings["tokenSecret"]),
                        new KeyValuePair { name = "msisdn", content = msisdn }, _fromForeground);

        }

        void client_GetDataFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
            {
                SetTile(true, "unavailable");
                return;
            }
            if (string.IsNullOrEmpty(args.Json) || string.Equals(args.Json, "[]"))
                return;
            try
            {
                _balance.Load(args.Json);
                SetTile(false, string.Empty);
            }
            catch (Exception)
            {
                SetTile(true, "error");
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: Tile has been updated");
#endif
        }

        private void SetTile(bool failed, string backContent)
        {
            if (_fromForeground)
            {
                SaveImageForeground(failed, backContent);
            }
            else
            {
                SaveImageBackground(failed, backContent);
            }
            var newTile = new FlipTileData
            {
                Count = _balance.Remaining,
                BackBackgroundImage = new Uri("isostore:" + Filename, UriKind.Absolute),
                BackContent = string.Empty,
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }

        public void SaveImageBackground(bool failed, string backcontent)
        {
            var i = 0;
            while (i < 3)
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => SaveImageForeground(failed, backcontent));
                }
                catch (Exception)
                {
                    i++;
                    continue;
                }
                i = 3;
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: From background: Image created");
#endif
        }

        private void SaveImageForeground(bool failed, string backcontent)
        {
            var i = 0;
            while (i < 3)
            {
                try
                {
                    var color = (bool) IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                        ? (SolidColorBrush) Application.Current.Resources["PhoneAccentBrush"]
                        : new SolidColorBrush(new Color {A = 255, R = 150, G = 8, B = 8});
                    Tile customTile;
                    if (failed)
                    {
                        customTile = new Tile(color, backcontent, string.Empty, string.Empty, string.Empty, string.Empty);
                    }
                    else if (_balance.Data != null)
                    {
                        customTile = new Tile(color, _balance.Credit, _balance.Data, _balance.Sms, _balance.VikingSms,
                            _balance.VikingMinutes);
                    }
                    else
                    {
                        customTile = new Tile(color, _balance.Credit, "0 MB", "0 SMS", _balance.VikingMinutes,
                            string.Empty);
                    }

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

                        using (var stream = isf.OpenFile(Filename, FileMode.OpenOrCreate))
                        {
                            bmp.SaveJpeg(stream, 336, 366, 0, 100);
                        }
                    }
                }
                catch (Exception)
                {
                    i++;
                    continue;
                }
                i = 3;
            }
        }
    }
}
