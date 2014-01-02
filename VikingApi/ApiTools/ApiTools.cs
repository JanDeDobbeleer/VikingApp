using System;
using Microsoft.Phone.Controls.Primitives;
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
            return date.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static string ToErrorMessage(this string message)
        {
            if (message.Contains("400"))
            {
                if (message.Contains("invalid consumer"))
                    return "The consumer key is no longer valid. Please contact the developer";
                if (message.Contains("invalid request token"))
                    return "The request token granted by the Mobile Vikings API is not valid.";
                if (message.Contains("could not verify"))
                    return "We could not verify your request. Please try again later.";
                if (message.Contains("invalid oauth verifier"))
                    return "The oauth verifier is invalid. Please try again later.";
                if (message.Contains("missing oauth parameters"))
                    return "The request seems to be missing a few parameters. Please try again later.";
            } 
            else if (message.Contains("403"))
            {
                if (message.Contains("xauth not allowed for this consumer"))
                    return "Fuel's xAuth extension permission has been revoked. Please contact the developer.";
                if (message.Contains("xauth username/password combination invalid"))
                    return "Your Username/password combination is incorrect. Please try again.";
            }
            else if (message.Contains("404"))
            {
                return "The service you are trying to reach no longer exists.";
            }
            else if (message.Contains("401") && message.Contains("www-authenticate"))
            {
                return "Your access token has expired, please login again using the logout function in settings.";
            }
            return message.Split(':')[0];
        }
    }
}