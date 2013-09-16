using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Coding4Fun.Toolkit.Controls;

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
                TextWrapping = TextWrapping.Wrap
            };
            toast.Show();
        }
    }
}