using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public BackTile(SolidColorBrush background, string credit)
        {
            InitializeComponent();
            MainGrid.Background = background;
            Credit.Text = credit;
            Right2.Text = string.Empty;
            Right3.Text = string.Empty;
            Right4.Text = string.Empty;
            Right5.Text = string.Empty;
        }

        public static void SaveTile(bool failed, UserBalance balance, string backcontent)
        {
            var i = 0;
            while (i < 5)
            {
                try
                {
                    var color = (bool) IsolatedStorageSettings.ApplicationSettings["tileAccentColor"]
                        ? (SolidColorBrush)Application.Current.Resources["TransparentBrush"]
                        : new SolidColorBrush(new Color {A = 255, R = 150, G = 8, B = 8});
                    BackTile customBackTile;
                    if (failed)
                    {
                        customBackTile = new BackTile(color, backcontent, string.Empty, string.Empty, string.Empty,
                            string.Empty)
                        {
                            Right1 = {Text = string.Empty},
                            Right2 = {Text = string.Empty},
                            Right3 = {Text = string.Empty},
                            Right4 = {Text = string.Empty},
                            Right5 = {Text = string.Empty}
                        };
                    }
                    else if (balance.Data != null)
                    {
                        customBackTile = new BackTile(color, balance.Credit, balance.Data, balance.Sms,
                            balance.VikingSms, balance.VikingMinutes);
                    }
                    else
                    {
                        customBackTile = new BackTile(color, balance.Credit);
                    }
                    if (!Helper.SaveElement(customBackTile, Tile.Back))
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
                }
                i = 5;
            }
        }
    }
}
