using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace VikingApi.ApiTools
{
    public struct KeyValuePair
    {
        public string name;
        public object content;
    }

    public static class ApiTools
    {
        public static bool HasInternetConnection()
        {
            return (Microsoft.Phone.Net.NetworkInformation.NetworkInterface.NetworkInterfaceType != Microsoft.Phone.Net.NetworkInformation.NetworkInterfaceType.None);
        }

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
    }
}
