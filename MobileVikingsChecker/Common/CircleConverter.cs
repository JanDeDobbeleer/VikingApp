using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Fuel.Common
{
    public class CircleConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as int? != null ? ReturnInformation((int)value) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private SolidColorBrush ReturnInformation(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return (SolidColorBrush)Application.Current.Resources["VikingColorBrush"];
                case 2:
                    return (SolidColorBrush)Application.Current.Resources["DataColorBrush"];
                case 5:
                    return (SolidColorBrush)Application.Current.Resources["SmsColorBrush"];
                case 7:
                    return (SolidColorBrush)Application.Current.Resources["SmsColorBrush"];
                case 11:
                    return (SolidColorBrush)Application.Current.Resources["VikingColorBrush"];
                case 15:
                    return (SolidColorBrush)Application.Current.Resources["VikingColorBrush"];
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }
    }
}
