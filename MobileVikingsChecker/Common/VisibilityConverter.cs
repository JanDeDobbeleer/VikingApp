using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using VikingApi.Json;

namespace Fuel.Common
{
    public class VisibilityConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Usage != null ? ReturnInformation(value as Usage) : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private Visibility ReturnInformation(Usage usage)
        {
            if (usage.IsVoice)
                return Visibility.Visible;
            return (usage.Price == "0.00") ? Visibility.Collapsed: Visibility.Visible;
        }
    }
}
