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
    public partial class BackTile : UserControl
    {
        public BackTile(SolidColorBrush background, string credit, string data, string sms, string vSms, string vCall)
        {
            InitializeComponent();
            MainGrid.Background = background;
            Credit.Text = credit;
            Data.Text = data;
            Sms.Text = sms;
            VSms.Text = vSms;
            VCall.Text = vCall;
        }

        public static void SaveTile(bool failed, UserBalance balance, string backcontent, string path, out bool crashed)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    var color = (bool)IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                        ? (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"]
                        : new SolidColorBrush(new Color { A = 255, R = 150, G = 8, B = 8 });
                    BackTile customBackTile;
                    if (failed)
                    {
                        customBackTile = new BackTile(color, backcontent, string.Empty, string.Empty, string.Empty, string.Empty);
                    }
                    else if (balance.Data != null)
                    {
                        customBackTile = new BackTile(color, balance.Credit, balance.Data, balance.Sms, balance.VikingSms, balance.VikingMinutes);
                    }
                    else
                    {
                        customBackTile = new BackTile(color, balance.Credit, "0 MB", "0 SMS", balance.VikingMinutes, string.Empty);
                    }
                    customBackTile.Measure(new Size(336, 336));
                    customBackTile.Arrange(new Rect(0, 0, 336, 336));
                    var bmp = new WriteableBitmap(336, 336);
                    bmp.Render(customBackTile, null);
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
