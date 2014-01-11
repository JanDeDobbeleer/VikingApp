using System;
using System.IO.IsolatedStorage;
using System.Linq;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace Tools
{
    public struct KeyValuePair
    {
        public object Content;
        public string Name;
    }

    public static class Tools
    {
        public static bool SaveSetting(KeyValuePair[] keyValuePair)
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

        public static bool SaveSetting(KeyValuePair keyValuePair)
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

        public static void SetProgressIndicator(bool isVisible)
        {
            SystemTray.ProgressIndicator.IsIndeterminate = isVisible;
            SystemTray.ProgressIndicator.IsVisible = isVisible;
        }

        public static ApplicationBarIconButton CreateButton(string uri, string text, bool enabled, EventHandler handler)
        {
            var button = new ApplicationBarIconButton { IconUri = new Uri(uri, UriKind.Relative), Text = text, IsEnabled = enabled };
            button.Click += handler;
            return button;
        }

        public static ApplicationBarMenuItem CreateMenuItem(string text, bool enabled, EventHandler handler)
        {
            var appBarMenuItem = new ApplicationBarMenuItem { Text = text, IsEnabled = enabled };
            appBarMenuItem.Click += handler;
            return appBarMenuItem;
        }

        public static bool HasInternetConnection()
        {
            return (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None);
        }

        public static void DefaultAllSettings()
        {
            IsolatedStorageSettings.ApplicationSettings["topup"] = true;
            IsolatedStorageSettings.ApplicationSettings["defaultnumber"] = string.Empty;
            IsolatedStorageSettings.ApplicationSettings["defaulttopupvalue"] = 10;
            IsolatedStorageSettings.ApplicationSettings["lastusedsim"] = false;
            IsolatedStorageSettings.ApplicationSettings["sim"] = string.Empty;
            IsolatedStorageSettings.ApplicationSettings["tileAccentColor"] = true;
            IsolatedStorageSettings.ApplicationSettings["oldtilestyle"] = false;
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
        }

        public static void ResetLiveTile()
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"])
            {
                BuildTile("/Assets/336x336.png","/Assets/336x336empty.png", "/Assets/159x159.png");
            }
            else
            {
                BuildTile("/Assets/336x336red.png", "/Assets/336x336redempty.png", "/Assets/159x159red.png");
            }
            UpdateLiveTile();
        }

        public static void UpdateLiveTile()
        {
            //remove all background tasks.
            foreach (var pt in ScheduledActionService.GetActions<PeriodicTask>())
            {
                ScheduledActionService.Remove(pt.Name);
            }
            var update = new UpdateLiveTile.UpdateLiveTile();
            update.Start(true);
        }



        public static void BuildTile(string frontImageUri, string backImageUri, string smallBackImageUri)
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