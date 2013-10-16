using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using VikingApi.ApiTools;

namespace Fuel.View
{
    public partial class SimPage : PhoneApplicationPage
    {
        public SimPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            App.Viewmodel.SimViewmodel.GetInfoFinished += SimViewmodelOnGetInfoFinished;
            App.Viewmodel.SimViewmodel.GetPlanInfoFinished += SimViewmodel_GetPlanInfoFinished;
            App.Viewmodel.SimViewmodel.GetSimInfoFinished += SimViewmodel_GetSimInfoFinished;
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/refresh.png", "refresh", true, RefreshOnClick));
        }

        private void RefreshOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.SimViewmodel.CancelTask();
            OnNavigatedTo(null);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            App.Viewmodel.SimViewmodel.RenewToken();
            await App.Viewmodel.SimViewmodel.GetTopUps(new DateTime(2009, 1, 1), DateTime.Now);
        }

        #region eventhandling
        private async void SimViewmodelOnGetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            RefreshListBox();
            TopUpViewer.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
            await App.Viewmodel.SimViewmodel.GetPlan();
        }

        private async void SimViewmodel_GetPlanInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if(args.Canceled)
                return;
            PlanTextBlock.Text = App.Viewmodel.SimViewmodel.Plan.Name;
            RefreshListBoxPlan();
            PricePlan.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
            await App.Viewmodel.SimViewmodel.GetSimInfo();
        }

        void SimViewmodel_GetSimInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            CardNumber.Text = App.Viewmodel.SimViewmodel.Card.CardNumber;
            Pin1.Text = App.Viewmodel.SimViewmodel.Card.Pin1;
            Pin2.Text = App.Viewmodel.SimViewmodel.Card.Pin2;
            Puk1.Text = App.Viewmodel.SimViewmodel.Card.Puk1;
            Puk2.Text = App.Viewmodel.SimViewmodel.Card.Puk2;
            IMSI.Text = App.Viewmodel.SimViewmodel.Card.Imsi;
            CardPanel.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
        }
        #endregion

        private void RefreshListBox()
        {
            TopUpViewer.ItemsSource = null;
            TopUpViewer.ItemsSource = App.Viewmodel.SimViewmodel.Topup;
        }

        private void RefreshListBoxPlan()
        {
            ListBoxBundles.ItemsSource = null;
            ListBoxPrices.ItemsSource = null;
            ListBoxBundles.ItemsSource = App.Viewmodel.SimViewmodel.Plan.Bundles;
            ListBoxPrices.ItemsSource = App.Viewmodel.SimViewmodel.Plan.Prices;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Viewmodel.SimViewmodel.CancelTask();
        }
    }
}