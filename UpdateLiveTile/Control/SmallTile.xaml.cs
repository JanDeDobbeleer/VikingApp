﻿using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UpdateLiveTile.Classes;

namespace UpdateLiveTile.Control
{
    public partial class SmallTile : UserControl
    {
        public SmallTile(SolidColorBrush background, string days)
        {
            InitializeComponent();
            LayoutRoot.Background = background;
            Days.Text = days;
            if (days.Equals("?") || int.Parse(days) < 10)
            {
                Days.Margin = new Thickness(0, -60, 29, 0);
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
                    SmallTile customSmallTile;
                    if (failed)
                    {
                        customSmallTile = new SmallTile(color, "?");
                    }
                    else if (balance.Data != null)
                    {
                        customSmallTile = new SmallTile(color, string.IsNullOrWhiteSpace(balance.Remaining.ToString()) ? "?" : balance.Remaining.ToString());
                    }
                    else
                    {
                        customSmallTile = new SmallTile(color, "0");
                    }
                    if (!Helper.SaveElement(customSmallTile, Tile.Small))
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
