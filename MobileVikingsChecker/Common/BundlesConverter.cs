using System;
using System.Globalization;
using System.Windows.Data;
using VikingApi.Json;

namespace Fuel.Common
{
    public class BundlesConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Bundle != null ? ReturnInformation(value as Bundle) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private string ReturnInformation(Bundle bundle)
        {
            return ConvertAmount(bundle) + " " +  ConvertType(bundle.type_id);
        }

        public static string ConvertType(int typeId)
        {
            switch (typeId)
            {
                case 1:
                    return "Call";
                case 2:
                    return "MB";
                case 5:
                    return "SMS";
                case 7:
                    return "MMS";
                case 11:
                    return "vCall";
                case 15:
                    return "vSMS";
                default:
                    return "Unknown";
            }
        }

        private string ConvertAmount(Bundle bundle)
        {
            switch (bundle.type_id)
            {
                case 11:
                    return (bundle.amount/60).ToString() + "m";
                default:
                    return bundle.amount.ToString();
            }
        }
    }
}
