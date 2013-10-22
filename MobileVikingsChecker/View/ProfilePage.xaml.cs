using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Tools;
using VikingApi.ApiTools;

namespace Fuel.View
{
    public partial class ProfilePage : PhoneApplicationPage
    {
        public ProfilePage()
        {
            InitializeComponent();
            BuildApplicationBar();
            App.Viewmodel.ProfileViewmodel.GetStatsFinished += SimViewmodelOnGetStatsFinished;
            App.Viewmodel.ProfileViewmodel.GetReferralFinished += SimViewmodel_GetReferralFinished;
            App.Viewmodel.ProfileViewmodel.GetLinkFinished += SimViewmodel_GetLinksFinished;
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/share.png", "referral", true, ShareOnClick));
            ((ApplicationBarIconButton) ApplicationBar.Buttons[1]).IsEnabled = false;
        }

        private void RefreshOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.ProfileViewmodel.CancelTask();
            OnNavigatedTo(null);
        }

        private void ShareOnClick(object sender, EventArgs e)
        {
            if (App.Viewmodel.ProfileViewmodel.Links == null || string.IsNullOrWhiteSpace(App.Viewmodel.ProfileViewmodel.Msisdn))
                return;
            var shareLinkTask = new ShareLinkTask
                {
                    LinkUri = new Uri(App.Viewmodel.ProfileViewmodel.Links.Select(x => x.Link).First(), UriKind.Absolute),
                    Message = "Join the revolution, become a viking! #fuel"
                };
            shareLinkTask.Show();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!Tools.Tools.HasInternetConnection())
            {
                Message.ShowToast("Bummer, it looks like we're all out of internet.");
                return;
            }
            SystemTray.ProgressIndicator = new ProgressIndicator();
            App.Viewmodel.ProfileViewmodel.RenewToken();
            await App.Viewmodel.ProfileViewmodel.GetStats();
        }

        #region eventhandling
        private async void SimViewmodelOnGetStatsFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            Remaining.Text = App.Viewmodel.ProfileViewmodel.Stats.UnusedPoints.ToString();
            Used.Text = App.Viewmodel.ProfileViewmodel.Stats.UsedPoints.ToString();
            Waiting.Text = App.Viewmodel.ProfileViewmodel.Stats.WaitingPoints.ToString();
            Topups.Text = App.Viewmodel.ProfileViewmodel.Stats.Topupsused.ToString();
            Earned.Text = App.Viewmodel.ProfileViewmodel.Stats.EarnedPoints.ToString();
            Tools.Tools.SetProgressIndicator(false);
            StatsPanel.Visibility = Visibility.Visible;
            await App.Viewmodel.ProfileViewmodel.GetReferrals();
        }

        private async void SimViewmodel_GetReferralFinished(object sender, GetInfoCompletedArgs args)
        {
            if(args.Canceled)
                return;
            RefreshListBox();
            ReferralViewer.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
            await App.Viewmodel.ProfileViewmodel.GetLinks();
        }

        void SimViewmodel_GetLinksFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            ((ApplicationBarIconButton)ApplicationBar.Buttons[1]).IsEnabled = true;
            Tools.Tools.SetProgressIndicator(false);
        }
        #endregion

        private void RefreshListBox()
        {
            ReferralViewer.ItemsSource = null;
            ReferralViewer.ItemsSource = App.Viewmodel.ProfileViewmodel.Referral;
        }
        
    }
}