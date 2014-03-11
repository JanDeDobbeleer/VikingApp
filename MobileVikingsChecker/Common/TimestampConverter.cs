using System;
using System.Globalization;
using System.Windows.Data;
using Fuel.Localization.Resources;
using VikingApi.Json;

namespace Fuel.Common
{
    public class TimestampConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string != null ? ReturnTimestamp(value as string) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ReturnTimestamp(string timestamp)
        {
            var date = System.Convert.ToDateTime(timestamp);
            return string.Format(AppResources.ConverterDateAtTimeFormat, date.ToShortDateString(), date.ToShortTimeString());
        }
    }
}
