using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using VikingApi.Json;

namespace Fuel.Common
{
    public class UsageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Usage != null ? ReturnBrush(value as Usage) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private SolidColorBrush ReturnBrush(Usage usage)
        {
            if (usage.IsSms || usage.IsMms)
                return (SolidColorBrush)Application.Current.Resources["SmsColorBrush"];
            if (usage.IsData)
                return (SolidColorBrush)Application.Current.Resources["DataColorBrush"];
            return (SolidColorBrush)Application.Current.Resources["VikingColorBrush"];
        }
    }
}
