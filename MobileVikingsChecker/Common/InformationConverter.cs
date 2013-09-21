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
            const string unit = "€";
            string information;
            if (usage.IsMms || usage.IsSms || usage.IsVoice)
            {
                var duration = string.Empty;
                var to = (usage.IsIncoming) ? "from":"to";
                if(usage.IsVoice)
                    duration = ReturnTimeSpan(usage);
                information = usage.IsVoice ? string.Format("{3} {0} {1} Duration: {2}", to, usage.To, duration, GetType(usage)) : string.Format("{0} {1}", to, usage.To);
            }
            else
            {
                information = string.Format("Data: {0}", (!usage.DurationHuman.Equals("n/a")) ? usage.DurationHuman : "0 MB");
            }
            var value = (usage.Price == "0.00")?string.Empty:unit + usage.Price;
            information += string.Format(" {0}", value);
            return information;
        }

        private string ReturnTimeSpan(Usage usage)
        {
            var timestamp = string.Empty;
            var difference = (System.Convert.ToDateTime(usage.EndTimestamp) - System.Convert.ToDateTime(usage.StartTimestamp));
            if (difference.Days > 0)
                timestamp += string.Format("{0} day{1}", difference.Days, (difference.Days == 1) ? "" : "s");
            if (difference.Hours > 0)
                timestamp += string.Format("{0} hour{1}", difference.Hours, (difference.Hours == 1) ? "" : "s");
            if (difference.Minutes > 0)
                timestamp += string.Format("{0} minute{1}", difference.Minutes, (difference.Minutes == 1) ? "" : "s");
            if (difference.Seconds > 0)
                timestamp += string.Format("{0} second{1}", difference.Seconds, (difference.Seconds == 1) ? "" : "s");
            return timestamp;
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
