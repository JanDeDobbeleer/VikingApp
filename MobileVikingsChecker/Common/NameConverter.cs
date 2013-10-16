using System;
using System.Globalization;
using System.Windows.Data;

namespace Fuel.Common
{
    public class NameConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string != null ? ReturnInformation(value as string) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ReturnInformation(string value)
        {
            return "by " + value;
        }
    }
}
