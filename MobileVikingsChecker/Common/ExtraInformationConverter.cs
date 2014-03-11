using System;
using System.Globalization;
using System.Windows.Data;
using Fuel.Localization.Resources;
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
                information += string.Format(AppResources.ConverterDurationFormat, ReturnTimeSpan(usage));
            }
            var value = (usage.Price == "0.00") ? string.Empty : unit + usage.Price;
            if (string.IsNullOrEmpty(value))
                return information;
            if (!string.IsNullOrEmpty(information))
                information += Environment.NewLine;
            information += string.Format(AppResources.ConverterCostFormat, value);
            return information;
        }

        private string ReturnTimeSpan(Usage usage)
        {
            var timestamp = string.Empty;
            var difference = (System.Convert.ToDateTime(usage.EndTimestamp) - System.Convert.ToDateTime(usage.StartTimestamp));
            if (difference.Days > 0)
                timestamp += string.Format(AppResources.ConverterDaysFormat, difference.Days).Space();
            if (difference.Hours > 0)
                timestamp += string.Format(AppResources.ConverterHoursFormat, difference.Hours).Space();
            if (difference.Minutes > 0)
                timestamp += string.Format(AppResources.ConverterMinutesFormat, difference.Minutes).Space();
            if (difference.Seconds > 0)
                timestamp += string.Format(AppResources.ConverterSecondsFormat, difference.Seconds);
            return timestamp;
        }
    }
}
