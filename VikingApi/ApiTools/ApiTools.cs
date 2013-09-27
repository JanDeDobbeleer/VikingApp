using System;
using Microsoft.Phone.Net.NetworkInformation;

namespace VikingApi.ApiTools
{
    public static class ApiTools
    {
        public static bool HasInternetConnection()
        {
            return (NetworkInterface.NetworkInterfaceType != NetworkInterfaceType.None);
        }

        public static string ToVikingApiTimeFormat(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddThh:mm:ss");
        }

        public static string ToErrorMessage(this string message)
        {
            return message.Split(':')[0];
        }
    }
}