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
                information = usage.IsVoice ? string.Format("{3} {0} {1} " + Environment.NewLine + "Duration: {2}", to, usage.To, duration, GetType(usage)) : string.Format("{2} {0} {1}", to, usage.To, GetType(usage));
            }
            else
            {
                information = string.Format("Data: {0}", (!usage.DurationHuman.Equals("n/a")) ? usage.DurationHuman : "0 MB");
            }
            var value = (usage.Price == "0.00")?string.Empty:unit + usage.Price;
            if(!string.IsNullOrEmpty(value))
                information += Environment.NewLine + string.Format("{0}", value);
            return information;
        }

        private string ReturnTimeSpan(Usage usage)
        {
            var timestamp = string.Empty;
            var difference = (System.Convert.ToDateTime(usage.EndTimestamp) - System.Convert.ToDateTime(usage.StartTimestamp));
            if (difference.Days > 0)
                timestamp += string.Format("{0}d", difference.Days);
            if (difference.Hours > 0)
                timestamp += string.Format("{0}h", difference.Hours);
            if (difference.Minutes > 0)
                timestamp += string.Format("{0}m", difference.Minutes);
            if (difference.Seconds > 0)
                timestamp += string.Format("{0}s", difference.Seconds);
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
