using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
                Days.Margin = new Thickness(0, -121, 61, 0);
                int result;
                if (int.TryParse(days, out result))
                {
                    DaysBlock.Text = (result == 1)? "day":"days";
                    DaysBlock.Margin = new Thickness(0, 5, 49, 0);
                }
            }
            else if (int.Parse(days) >= 10 && int.Parse(days) < 20)
            {
                switch (int.Parse(days))
                {
                    case 11:
                        Days.Margin = new Thickness(0, -121, 50, 0);
                        break;
                    default:
                        Days.Margin = new Thickness(0, -121, 42, 0);
                        break;
                }
            }
            else if (int.Parse(days) == 21)
            {
                    Days.Margin = new Thickness(0, -121, 42, 0);
            }
        }

        public static void SaveTile(bool failed, UserBalance balance)
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
                        customFrontTile = new FrontTile(color, string.IsNullOrWhiteSpace(balance.Remaining.ToString()) ? "?" : balance.Remaining.ToString());
                    }
                    else
                    {
                        customFrontTile = new FrontTile(color, "0");
                    }
                    if (!Helper.SaveElement(customFrontTile, Tile.Front))
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
                        return;
                    }
                    continue;
                }
                i = 5;
            }
        }
    }
}
