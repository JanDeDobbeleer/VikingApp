using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.UserData;
using VikingApi.Json;

namespace Fuel.Common
{
     public class InformationConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Usage != null ? ReturnInformation(value as Usage) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ReturnInformation(Usage usage)
        {
            string information;
            if (usage.IsMms || usage.IsSms || usage.IsVoice)
            {
                var to = (usage.IsIncoming) ? "from":"to";
                information = string.Format("{0} {1} {2} ", GetType(usage), to, FetchContactName(usage.To));
            }
            else
            {
                information = string.Format("data: {0}", (!usage.DurationHuman.Equals("n/a")) ? usage.DurationHuman : "0 MB");
            }
            return information;
        }

        private string GetType(Usage usage)
        {
            if (usage.IsMms)
                return "sms";
            if (usage.IsSms)
                return "sms";
            if (usage.IsVoice)
                return "call";
            return usage.IsData ? "data" : string.Empty;
        }

         private string FetchContactName(string numberStr)
         {
             if (App.Viewmodel.UsageViewmodel.Contacts == null)
                 return numberStr;
             var result = from Contact con in App.Viewmodel.UsageViewmodel.Contacts
                          from ContactPhoneNumber a in con.PhoneNumbers
                          where a.PhoneNumber.Contains((numberStr.StartsWith("0"))?numberStr.Remove(0,1):numberStr)
                          select con;
             var enumerable = result as IList<Contact> ?? result.ToList();
             if (!enumerable.Any()) 
                 return numberStr;
             var firstOrDefault = enumerable.FirstOrDefault();
             if (firstOrDefault != null) return firstOrDefault.DisplayName ?? numberStr;
             return numberStr;
         }
    }
}
