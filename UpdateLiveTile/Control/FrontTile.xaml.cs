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
                    if (!Helper.SaveElement(customFrontTile, path))
                    {
                        i++;
                        continue;
                    }
                }
                catch (Exception)
                {
                    i++;
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
