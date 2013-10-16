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
            App.Viewmodel.SimViewmodel.GetInfoFinished += SimViewmodelOnGetInfoFinished;
            App.Viewmodel.SimViewmodel.GetPlanInfoFinished += SimViewmodel_GetPlanInfoFinished;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            App.Viewmodel.SimViewmodel.RenewToken();
            await App.Viewmodel.SimViewmodel.GetTopUps(new DateTime(2009, 1, 1), DateTime.Now);
        }

        private async void SimViewmodelOnGetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            RefreshListBox();
            TopUpViewer.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
            await App.Viewmodel.SimViewmodel.GetPlan();
        }

        private void SimViewmodel_GetPlanInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if(args.Canceled)
                return;
            PlanTextBlock.Text = App.Viewmodel.SimViewmodel.Plan.Name;
            RefreshListBoxPlan();
            PricePlan.Visibility = Visibility.Visible;
            Tools.Tools.SetProgressIndicator(false);
        }

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