using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Fuel.Localization.Resources;
using Microsoft.Phone.Shell;
using VikingApi.Api;

namespace Fuel.LoginControl
{
    public delegate void LoginFinishedEventHandler(object sender, ApiBrowserEventArgs args);

    public partial class OauthLogin
    {
        private bool _panel;

        public OauthLogin()
        {
            InitializeComponent();
            _panel = false;
        }

        public event LoginFinishedEventHandler LoginFinished;

        protected void OnLoginFinished(ApiBrowserEventArgs args)
        {
            if (LoginFinished != null)
            {
                LoginFinished(this, args);
            }
        }

        private void OauthLogin_OnLoaded(object sender, RoutedEventArgs e)
        {
            var v = (Visibility)Resources["PhoneLightThemeVisibility"];
            VikingImage.Source = (v == Visibility.Visible) ? new BitmapImage(new Uri("/Assets/vikinglogoblack.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/vikinglogowhite.png", UriKind.Relative));
            QuestionMark.Source = (v == Visibility.Visible) ? new BitmapImage(new Uri("/Assets/questionmark.png", UriKind.Relative)) : new BitmapImage(new Uri("/Assets/questionmarklight.png", UriKind.Relative));
        }

        #region buttons
        private void Login_OnTap(object sender, RoutedEventArgs e)
        {
            if (!_panel)
            {
                VikingImage.Visibility = Visibility.Collapsed;
                StackPanel.Visibility = Visibility.Visible;
                UserName.Focus();
                _panel = true;
            }
            else
            {
                Login();
            }
        }
        #endregion

        private void PassWord_OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key.Equals(Key.Enter))
                Login();
        }

        private async void Login()
        {
            if (string.IsNullOrWhiteSpace(UserName.Text))
            {
                Tools.Message.ShowToast(AppResources.ToastUserName);
                return;
            }
            if (string.IsNullOrWhiteSpace(PassWord.Password))
            {
                Tools.Message.ShowToast(AppResources.ToastPassword);
                return;
            }

            Tools.Tools.SetProgressIndicator(true);
            SystemTray.ProgressIndicator.Text = AppResources.ProgressRequestingAccess;
            using (var client = new VikingsApi())
            {
                if (await client.Authorize(UserName.Text, PassWord.Password))
                {
                    var finished = new ApiBrowserEventArgs { Success = true };
                    OnLoginFinished(finished);
                    Visibility = Visibility.Collapsed;
                }
                else
                {
                    Tools.Message.ShowToast(AppResources.ToastLoginUnsuccessful);
                }
            }
        }

        private void UIElement_OnTap(object sender, GestureEventArgs e)
        {
            MessageBox.Show(AppResources.PopupPrivacy, AppResources.PopupPrivacyHeader, MessageBoxButton.OK);
        }
    }
}