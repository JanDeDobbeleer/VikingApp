using System;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
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
        const string FilenameBack = "/Shared/ShellContent/CustomTile.jpg";
        const string FilenameFront = "/Shared/ShellContent/CustomTileFront.jpg";
        private bool _fromForeground;
        private bool _backCrashed;
        private bool _frontCrashed;

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
            Debug.WriteLine("Live tile: BackTile has been updated");
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
                Count = 0,
                BackBackgroundImage = _backCrashed ? GetDefaultBackTile() : new Uri("isostore:" + FilenameBack, UriKind.Absolute),
                BackgroundImage = _frontCrashed ? GetDefaultFrontTile() : new Uri("isostore:" + FilenameFront, UriKind.Absolute),
                BackContent = string.Empty,
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }

        public void SaveImageBackground(bool failed, string backcontent)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() => SaveImageForeground(failed, backcontent));
                }
                catch (Exception)
                {
                    i++;
                }
                i = 5;
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: From background: Image created");
#endif
        }

        private void SaveImageForeground(bool failed, string backcontent)
        {
            FrontTile.SaveTile(failed, _balance, FilenameFront, out _frontCrashed);
            BackTile.SaveTile(failed, _balance, backcontent, FilenameBack, out _backCrashed);
        }

        private Uri GetDefaultBackTile()
        {
            return (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                            ? new Uri("/Assets/336x336empty.png", UriKind.RelativeOrAbsolute)
                            : new Uri("/Assets/336x336redempty.png", UriKind.RelativeOrAbsolute);
        }

        private Uri GetDefaultFrontTile()
        {
            return (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                            ? new Uri("/Assets/336x336.png", UriKind.RelativeOrAbsolute)
                            : new Uri("/Assets/336x336red.png", UriKind.RelativeOrAbsolute);
        }
    }
}
