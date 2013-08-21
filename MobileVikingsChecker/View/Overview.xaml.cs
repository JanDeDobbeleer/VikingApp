using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MobileVikingsChecker.Common;

namespace MobileVikingsChecker.View
{
    public partial class Overview : PhoneApplicationPage
    {
        public Overview()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 0, IsVisible = true, ForegroundColor = (Color)Application.Current.Resources["VikingColor"] };
            ApplicationBar.Buttons.Add(Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.CreateButton("/Assets/add.png", "reload", true, ReloadOnClick));
        }

        private void RefreshOnClick(object sender, EventArgs e)
        {
            //TODO: add logic to fetch new data
        }

        private void ReloadOnClick(object sender, EventArgs e)
        {
            //TODO: add logic to fetch new data
        }
    }
}