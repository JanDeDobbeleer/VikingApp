using System;
using System.Globalization;
using System.Windows.Data;
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
                information = string.Format("{0} {1} {2} ", GetType(usage), to, usage.To);
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
    }
}
