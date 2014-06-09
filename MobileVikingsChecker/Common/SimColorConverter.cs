using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Fuel.Common
{
    public class SimColorConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value as string != null)
                return Application.Current.Resources["VikingColorBrush"];
            return Application.Current.Resources["VikingColorBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
