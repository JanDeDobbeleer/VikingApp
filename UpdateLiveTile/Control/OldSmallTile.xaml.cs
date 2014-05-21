using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UpdateLiveTile.Classes;

namespace UpdateLiveTile.Control
{
    public partial class OldSmallTile : UserControl
    {
        public OldSmallTile(SolidColorBrush background, string days)
        {
            InitializeComponent();
            LayoutRoot.Background = background;
            Days.Text = days;
        }

        public static void SaveTile(bool failed, UserBalance balance)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    var color = (bool)IsolatedStorageSettings.ApplicationSettings[Setting.FrontTileAccentColor.ToString()]
                        ? (SolidColorBrush)Application.Current.Resources["TransparentBrush"]
                        : new SolidColorBrush(new Color { A = 255, R = 150, G = 8, B = 8 });
                    OldSmallTile customSmallTile;
                    if (failed)
                    {
                        customSmallTile = new OldSmallTile(color, "?");
                    }
                    else if (balance.Data != null)
                    {
                        customSmallTile = new OldSmallTile(color, string.IsNullOrWhiteSpace(balance.Remaining.ToString()) ? "?" : balance.Remaining.ToString());
                    }
                    else
                    {
                        customSmallTile = new OldSmallTile(color, "0");
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
