using System;
using System.IO.IsolatedStorage;
using System.Linq;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace Tools
{
    public struct KeyValuePair
    {
        public object Content;
        public string Name;
    }

    public enum Setting
    {
        Topup,
        Defaultnumber,
        Defaulttopupvalue,
        Lastusedsim,
        Sim,
        FrontTileAccentColor,
        BackTileAccentColor,
        Oldtilestyle,
        Login,
        TokenKey,
        TokenSecret
    }

    public class Settings
    {
        #region Constructor
        private static Settings _sInstance;
        private static readonly object SInstanceSync = new object();

        protected Settings()
        {
        }

        /// <summary>
        /// Returns an instance (a singleton)
        /// </summary>
        /// <returns>a singleton</returns>
        /// <remarks>
        /// This is an implementation of the singelton design pattern.
        /// </remarks>
        public static Settings GetInstance()
        {
            // This implementation of the singleton design pattern prevents 
            // unnecessary locks (using the double if-test)
            if (_sInstance == null)
            {
                lock (SInstanceSync)
                {
                    if (_sInstance == null)
                    {
                        _sInstance = new Settings();
                    }
                }
            }
            return _sInstance;
        }
        #endregion

        public bool SaveSetting(KeyValuePair[] keyValuePair)
        {
            try
            {
                for (int i = 0; i < keyValuePair.Count(); i++)
                {
                    SaveSetting(keyValuePair[i]);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SaveSetting(KeyValuePair keyValuePair)
        {
            try
            {
                IsolatedStorageSettings.ApplicationSettings[keyValuePair.Name] = keyValuePair.Content;
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public void AssignSettings(bool reset = false)
        {
            foreach (var setting in Enum.GetValues(typeof(Setting)).Cast<object>().Where(setting => !IsolatedStorageSettings.ApplicationSettings.Contains(setting.ToString())))
            {
                DefaultSetting((Setting)setting);
            }
            if(reset)
                DefaultTile();
        }

        private void DefaultSetting(Setting setting)
        {
            switch (setting)
            {
                case Setting.Topup:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = true;
                    break;
                case Setting.Defaultnumber:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = string.Empty;
                    break;
                case Setting.Defaulttopupvalue:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = 10;
                    break;
                case Setting.Lastusedsim:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = false;
                    break;
                case Setting.Sim:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = string.Empty;
                    break;
                case Setting.FrontTileAccentColor:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = true;
                    break;
                case Setting.BackTileAccentColor:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = true;
                    break;
                case Setting.Oldtilestyle:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = false;
                    break;
                case Setting.Login:
                    IsolatedStorageSettings.ApplicationSettings[setting.ToString()] = true;
                    break;
            }
        }

        private void DefaultTile()
        {
            var newTile = new FlipTileData
            {
                BackContent = string.Empty,
                Count = 0,
                BackBackgroundImage = new Uri("/Assets/336x336empty.png", UriKind.Relative)
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
            ResetLiveTile();
            //remove all background tasks.
            foreach (var pt in ScheduledActionService.GetActions<PeriodicTask>())
            {
                ScheduledActionService.Remove(pt.Name);
            }
        }

        public void ResetLiveTile()
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"])
            {
                BuildTile("/Assets/336x336.png", "/Assets/336x336empty.png", "/Assets/159x159.png");
            }
            else
            {
                BuildTile("/Assets/336x336red.png", "/Assets/336x336redempty.png", "/Assets/159x159red.png");
            }
            UpdateLiveTile();
        }

        public void UpdateLiveTile()
        {
            var update = new UpdateLiveTile.UpdateLiveTile();
            update.Start(true);
        }

        private void BuildTile(string frontImageUri, string backImageUri, string smallBackImageUri)
        {
            var newTile = new FlipTileData
            {
                BackgroundImage = new Uri(frontImageUri, UriKind.Relative),
                BackBackgroundImage = new Uri(backImageUri, UriKind.Relative),
                SmallBackgroundImage = new Uri(smallBackImageUri, UriKind.Relative)
            };
            var firstOrDefault = ShellTile.ActiveTiles.FirstOrDefault();
            if (firstOrDefault != null)
                firstOrDefault.Update(newTile);
        }
    }
}
