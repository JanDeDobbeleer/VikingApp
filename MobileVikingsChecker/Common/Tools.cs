using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

namespace MobileVikingsChecker.Common
{
    public struct KeyValuePair
    {
        public string name;
        public object content;
    }

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
    }
}
