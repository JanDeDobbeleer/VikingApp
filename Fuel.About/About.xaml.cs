using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using Windows.System;
using Fuel.Localization.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

namespace Fuel.About
{
    public partial class About : PhoneApplicationPage
    {
        private StackPanel _licenses;

        public About()
        {
            InitializeComponent();
            AssignValues();
        }

        private void AssignValues()
        {
            try
            {
                GetVersionNumber();
                LoadLicense();
                HyperLinkButton.NavigateUri = new Uri("http://www.jan-joris.be");
                ReviewBlock.Xaml = @AppResources.AboutViewDisclaimer;
                GetVikingNumbers();
            }
            catch (Exception)
            {
                //TODO: handle this
            }
        }

        #region buttons

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var s = ((ButtonBase)sender).Tag as string;
            switch (s)
            {
                case "Review":
                    var task = new MarketplaceReviewTask();
                    task.Show();
                    break;
            }
        }

        #endregion

        #region functions

        private void GetVersionNumber()
        {
            VersionText.Text = Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0] ?? AppResources.AboutViewUnknownAssembly;
        }

        private void LoadLicense()
        {
            if (_licenses != null)
                return;
            _licenses = new StackPanel();
            var xaml = @AppResources.AboutViewMobileVikings;
            _licenses.Children.Add(new RichTextBox { Xaml = @AppResources.AboutViewMobileVikings });
            _licenses.Children.Add(ReturnHyperlinkButton());
            Sv1.Content = _licenses;
        }

        private HyperlinkButton ReturnHyperlinkButton()
        {
            var link = new HyperlinkButton
            {
                Content = "Mobile Vikings",
                NavigateUri = new Uri("https://mobilevikings.com/bel/en/", UriKind.RelativeOrAbsolute),
                TargetName = "blank",
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 10, 0, 0)
            };
            return link;
        }


        private bool GetVikingNumbers()
        {
            try
            {
                if (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.None)
                    return false;
                var client = new WebClient();
                client.DownloadStringCompleted += ClientOnDownloadStringCompleted;
                client.DownloadStringAsync(new Uri("https://mobilevikings.com/api/active_users", uriKind: UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs downloadStringCompletedEventArgs)
        {
            try
            {
                var textblock = new TextBlock
                {
                    Text = "There are currently " + downloadStringCompletedEventArgs.Result + " vikings.",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                _licenses.Children.Insert(8, textblock);
            }
            catch (Exception)
            {
            }
        }

        #endregion

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var emailComposeTask = new EmailComposeTask
            {
                Subject = "Feedback from Fuel for Mobile Vikings",
                To = "vikingapp@outlook.com"
            };
            emailComposeTask.Show();
        }

        private async void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            //zune:search?publisher=[publisher name]
            await Launcher.LaunchUriAsync(new Uri("zune:search?publisher=Jan Joris"));
        }
    }
}