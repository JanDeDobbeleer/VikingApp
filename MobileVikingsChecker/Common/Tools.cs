using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Shell;
using VikingApi.ApiTools;

namespace Fuel.Common
{
    static class Tools
    {
        public static bool SaveSetting(KeyValuePair[] keyValuePair)
        {
            try
            {
                for (int i = 0; i < keyValuePair.Count(); i++)
                {
                    IsolatedStorageSettings.ApplicationSettings[keyValuePair[i].name] = keyValuePair[i].content;
                    IsolatedStorageSettings.ApplicationSettings.Save();
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
                IsolatedStorageSettings.ApplicationSettings[keyValuePair.name] = keyValuePair.content;
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

        public static bool HasInternetConnection()
        {
            return (Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType != Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None);
        }

        public static bool HandleError(object result, string message)
        {
            if (result != null)
                return false;
            ShowToast(message);
            return true;
        }

        public static void ShowToast(string message)
        {
            SetProgressIndicator(false);
            var toast = new ToastPrompt
            {
                Title = "Fuel",
                Message = message,
                ImageSource = new BitmapImage(new Uri("/Assets/ToastIcon.png", UriKind.RelativeOrAbsolute)),
                MillisecondsUntilHidden = 3000,
                TextOrientation = Orientation.Vertical
            };
            toast.Show();
        }
    }
}
