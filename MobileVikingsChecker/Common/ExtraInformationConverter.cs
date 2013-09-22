using System;
using System.Globalization;
using System.Windows.Data;
using VikingApi.Json;

namespace Fuel.Common
{
    public class ExtraInformationConverter:IValueConverter
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
            var information = string.Empty;
            if (usage.IsVoice)
            {
                information += string.Format("duration: {0}", ReturnTimeSpan(usage));
            }
            var value = (usage.Price == "0.00") ? string.Empty : unit + usage.Price;
            if (string.IsNullOrEmpty(value))
                return information;
            if (!string.IsNullOrEmpty(information))
                information += Environment.NewLine;
            information += string.Format("cost: {0}", value);
            return information;
        }

        private string ReturnTimeSpan(Usage usage)
        {
            var timestamp = string.Empty;
            var difference = (System.Convert.ToDateTime(usage.EndTimestamp) - System.Convert.ToDateTime(usage.StartTimestamp));
            if (difference.Days > 0)
                timestamp += string.Format("{0}d ", difference.Days);
            if (difference.Hours > 0)
                timestamp += string.Format("{0}h ", difference.Hours);
            if (difference.Minutes > 0)
                timestamp += string.Format("{0}m ", difference.Minutes);
            if (difference.Seconds > 0)
                timestamp += string.Format("{0}s", difference.Seconds);
            return timestamp;
        }
    }
}
