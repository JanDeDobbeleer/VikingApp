using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using VikingApi.Json;

namespace Fuel.Common
{
    public class PricesConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
#if(DEBUG)
            Debug.WriteLine("Price Converter: converting typeid: " + (value as Price).type_id);
#endif
            return value as Price != null ? ReturnInformation(value as Price) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ReturnInformation(Price price)
        {
            return BundlesConverter.ConvertType(price.type_id) + ": " + AmountConverter.ReturnInformation(price.amount);
        }
    }
}
