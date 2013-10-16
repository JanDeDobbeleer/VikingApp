using System;
using System.Globalization;
using System.Windows.Data;

namespace Fuel.Common
{
    public class AmountConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as string != null ? ReturnInformation(value as string) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static string ReturnInformation(string amount)
        {
            const string unit = "€";
            var firstAmount = int.Parse(amount.Split('.')[0]);
            if (firstAmount > 0)
                return unit + firstAmount;
            return unit + amount;
        }
    }
}
