﻿using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Resources;
using System.Windows.Shapes;
using Windows.System;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

namespace Fuel.About
{
    public partial class About : PhoneApplicationPage
    {
        private StackPanel _licenses;
        private const string Disclaimer = "Hi there. Welcome to Fuel. This app has been built with passion, a lot of cursing, crying and occasional eufory. So, when you encounter an issue, please hit the contact button instead of handing out a 1 star review.\n\nFuel will remain free throughout it's lifecycle, all I ask is that you download my other apps (which you may immediately delete) so that I can get my stats up to par and receive some freebies to support all this development. Tap my name on the top of this page to reveal my apps.";

        public About()
        {
            InitializeComponent();
            AssignValues();
        }

        private void AssignValues()
        {
            GetVersionNumber();
            LoadLicense();
            HyperLinkButton.NavigateUri = new Uri("http://www.jan-joris.be");
            ReviewBlock.Text = Disclaimer;
            GetVikingNumbers();
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
            VersionText.Text = Assembly.GetExecutingAssembly().FullName.Split('=')[1].Split(',')[0] ?? "Unknown";
        }

        private void LoadLicense()
        {
            if (_licenses == null)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    _licenses = new StackPanel();

                    var sri = Application.GetResourceStream(
                        new Uri("OVER.txt", UriKind.Relative));
                    if (sri != null)
                    {
                        using (var sr = new StreamReader(sri.Stream))
                        {
                            string line;
                            var lastWasEmpty = true;
                            do
                            {
                                line = sr.ReadLine();

                                if (string.IsNullOrWhiteSpace(line))
                                {
                                    var r = new Rectangle
                                    {
                                        Height = 15,
                                    };
                                    _licenses.Children.Add(r);
                                    lastWasEmpty = true;
                                }
                                else
                                {
                                    var tb = new TextBlock
                                    {
                                        TextWrapping = TextWrapping.Wrap,
                                        Text = line,
                                        Style = (lastWasEmpty && line.Length > 30) ? SmallStyle : (Style)Application.Current.Resources["PhoneTextNormalStyle"],
                                    };
#if(DEBUG)
                                    Debug.WriteLine("Textblock: \n" + line);
#endif
                                    _licenses.Children.Add(tb);
                                    lastWasEmpty = false;
                                }
                            } while (line != null);
                        }
                    }
                    _licenses.Children.Add(ReturnHyperlinkButton());
                    Sv1.Content = _licenses;
                });
            }
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


        private void GetVikingNumbers()
        {
            if (NetworkInterface.NetworkInterfaceType == NetworkInterfaceType.None)
                return;
            var client = new WebClient();
            client.DownloadStringCompleted += ClientOnDownloadStringCompleted;
            client.DownloadStringAsync(new Uri("https://mobilevikings.com/api/active_users", uriKind: UriKind.RelativeOrAbsolute));
        }

        private void ClientOnDownloadStringCompleted(object sender, DownloadStringCompletedEventArgs downloadStringCompletedEventArgs)
        {
            var textblock = new TextBlock
                {
                    Text = "There are currently " + downloadStringCompletedEventArgs.Result + " vikings.",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
            _licenses.Children.Insert(8, textblock);
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