using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UpdateLiveTile.Classes;

namespace UpdateLiveTile.Control
{
    public partial class FrontTile : UserControl
    {
        public FrontTile(SolidColorBrush background, string days)
        {
            InitializeComponent();
            LayoutRoot.Background = background;
            Days.Text = days;
            if (days.Equals("?") || int.Parse(days) < 10)
            {
                Days.Margin = new Thickness(0, -138, 70, 0);
            }
        }

        public static void SaveTile(bool failed, UserBalance balance, string path)
        {
            try
            {
                var color = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                    ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]
                    : new SolidColorBrush(new Color { A = 255, R = 150, G = 8, B = 8 });
                var tile = GetElement(failed, balance, color);
                tile.Measure(new Size(336, 336));
                tile.Arrange(new Rect(0, 0, 336, 336));
                var bmp = new WriteableBitmap(336, 336);
                bmp.Render(tile, null);
                bmp.Invalidate();

                using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (!isf.DirectoryExists("/CustomLiveTiles"))
                    {
                        isf.CreateDirectory("/CustomLiveTiles");
                    }

                    using (var stream = isf.OpenFile(path, FileMode.OpenOrCreate))
                    {
                        bmp.SaveJpeg(stream, 336, 366, 0, 100);
                    }
                }
            }
            catch (Exception)
            {
                //sleep for 0.5 seconds in order to try to resolve the isolatedstorage issues
                Thread.Sleep(500);
                SaveTile(failed, balance, path);
            }
            if (!CheckFileSize(path, 27000))
                SaveTile(failed, balance, path);
        }

        private static UIElement GetElement(bool failed, UserBalance balance, SolidColorBrush color)
        {
            if (failed)
            {
                return new FrontTile(color, "?");
            }
            return balance.Data != null ? new FrontTile(color, balance.Remaining.ToString() ?? "?") : new FrontTile(color, "0");
        }

        private static bool CheckFileSize(string path, int size)
        {
            using (var isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isf.FileExists(path))
                    return false;
                var file = isf.OpenFile(path, FileMode.Open);
                if (file.Length >= size) 
                    return true;
                file.Close();
                return false;
            }
        }
    }
}
