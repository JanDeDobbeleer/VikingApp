using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;

namespace Tools
{
    public static class Message
    {
        public static void ShowToast(string message)
        {
            Tools.SetProgressIndicator(false);
            var toast = new ToastPrompt
            {
                Title = "Fuel",
                Message = message,
                ImageSource = new BitmapImage(new Uri("/Assets/ToastIcon.png", UriKind.RelativeOrAbsolute)),
                MillisecondsUntilHidden = 3000,
                TextOrientation = Orientation.Vertical, 
                TextWrapping = TextWrapping.Wrap,
                Background = (SolidColorBrush)Application.Current.Resources["VikingColorBrush"],
                Margin = new Thickness(0,0,0,-10)
                
            };
            toast.Show();
        }

        public static void SendErrorEmail(string error, string location)
        {
            if (MessageBox.Show("This is not supposed to appen, would you like to send a report?", "Oops", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return;
            var task = new EmailComposeTask 
                {
                    Subject = "Error report for Fuel", 
                    To = "vikingapp@outlook.com", 
                    Body = BuildErrorBody(error, location)
                };
            task.Show();
        }

        private static string BuildErrorBody(string error, string location)
        {
            var builder = new StringBuilder();
            builder.Append(string.Format("Error at {0}" + Environment.NewLine,location));
            builder.Append(DeviceInformation(builder));
            builder.Append(string.Format("Log:" + Environment.NewLine + "{0}", error));
            return builder.ToString();

        }

        private static string DeviceInformation(StringBuilder builder = null)
        {
            if(builder == null)
                builder = new StringBuilder();
            builder.Append(string.Format("Device: {0}" + Environment.NewLine, DeviceExtendedProperties.GetValue("DeviceManufacturer")));
            builder.Append(string.Format("Device name: {0}" + Environment.NewLine, DeviceExtendedProperties.GetValue("DeviceName")));
            builder.Append(string.Format("Firmware version: {0}" + Environment.NewLine, DeviceExtendedProperties.GetValue("DeviceFirmwareVersion")));
            builder.Append(string.Format("Current memory usage: {0}" + Environment.NewLine, DeviceExtendedProperties.GetValue("DeviceCurrentMemoryUsage")));
            builder.Append(string.Format("Peak memory usage: {0}" + Environment.NewLine, DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage")));
            return builder.ToString();
        }
    }
}