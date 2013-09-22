using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using VikingApi.Json;

namespace Fuel.Common
{
    public class UsageImageConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value as Usage != null ? ReturnImage(value as Usage) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage ReturnImage(Usage usage)
        {
            if (usage.IsSuperOnNet)
            {
                var helmet = (Visibility.Visible == (Visibility) Application.Current.Resources["PhoneDarkThemeVisibility"]) ? "/Assets/helmetWhite.png":"/Assets/helmetDark.png";
                return new BitmapImage(new Uri(helmet, UriKind.Relative));
            }
            return usage.IsIncoming ? new BitmapImage((new Uri((Visibility.Visible == (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"]) ? "/Assets/download.png" : "/Assets/downloaddark.png", UriKind.Relative))) : new BitmapImage((new Uri((Visibility.Visible == (Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"]) ? "/Assets/upload.png" : "/Assets/uploaddark.png", UriKind.Relative)));
        }
    }
}
