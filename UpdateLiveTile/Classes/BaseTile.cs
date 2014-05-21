using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UpdateLiveTile.Classes
{
    public abstract class BaseTile
    {
        public void SaveTile(bool failed, UserBalance balance, string backcontent, string path)
        {
            try
            {
                var color = (bool)IsolatedStorageSettings.ApplicationSettings[Setting.FrontTileAccentColor.ToString()]
                    ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]
                    : new SolidColorBrush(new Color { A = 255, R = 150, G = 8, B = 8 });
                var tile = GetElement();
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
                SaveTile(failed, balance, backcontent, path);
            }
        }

        protected abstract UIElement GetElement();
    }
}
