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
        private bool _fromForeground;
        private bool _running;

        public async void Start(bool fromForeground)
        {
            if(_running)
                return;
            _fromForeground = fromForeground;
            if (string.IsNullOrEmpty((string)IsolatedStorageSettings.ApplicationSettings["sim"]))
            {
                SetTile(true, "unavailable");
                return;
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: Start updating Live tile for number " + (string)IsolatedStorageSettings.ApplicationSettings["sim"]);
#endif
            _running = true;
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
            _running = false;
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
        }

        public void SaveImageBackground(bool failed, string backcontent)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        SaveImageForeground(failed, backcontent);
#if(DEBUG)
                        Debug.WriteLine("Live tile: From background: Images created");
#endif
                    });
                }
                catch (Exception)
                {
                    i++;
#if(DEBUG)
                    Debug.WriteLine("Live tile: From background: error");
#endif
                }
                i = 5;
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: From background: Live tile updated");
#endif
        }

        private void SaveImageForeground(bool failed, string backcontent)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("oldtilestyle") && (bool)IsolatedStorageSettings.ApplicationSettings["oldtilestyle"])
            {
                OldSmallTile.SaveTile(failed, _balance);
            }
            else
            {
                SmallTile.SaveTile(failed, _balance);
            }
#if(DEBUG)
            Debug.WriteLine("Live tile: Small image created");
#endif

            FrontTile.SaveTile(failed, _balance);
#if(DEBUG)
            Debug.WriteLine("Live tile: Front image created");
#endif
            BackTile.SaveTile(failed, _balance, backcontent);
#if(DEBUG)
            Debug.WriteLine("Live tile: Back image created");
#endif

        }
    }
}
