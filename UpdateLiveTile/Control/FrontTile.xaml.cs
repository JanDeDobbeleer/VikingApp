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

        public static void SaveTile(bool failed, UserBalance balance, string path, out bool crashed)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    var color = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                        ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]
                        : new SolidColorBrush(new Color { A = 255, R = 150, G = 8, B = 8 });
                    FrontTile customFrontTile;
                    if (failed)
                    {
                        customFrontTile = new FrontTile(color, "?");
                    }
                    else if (balance.Data != null)
                    {
                        customFrontTile = new FrontTile(color, balance.Remaining.ToString() ?? "?");
                    }
                    else
                    {
                        customFrontTile = new FrontTile(color, "0");
                    }
                    customFrontTile.Measure(new Size(336, 336));
                    customFrontTile.Arrange(new Rect(0, 0, 336, 336));
                    var bmp = new WriteableBitmap(336, 336);
                    bmp.Render(customFrontTile, null);
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
                    i++;
                    //sleep for 3 seconds in order to try to resolve the isolatedstorage issues
                    Thread.Sleep(3000);
                    if (i == 5)
                    {
                        crashed = true;
                        return;
                    }
                    continue;
                }
                i = 5;
            }
            crashed = false;
        }
    }
}
