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
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            App.Viewmodel.SimViewmodel.RenewToken();
            await App.Viewmodel.SimViewmodel.GetUsage(new DateTime(2009, 1, 1), DateTime.Now);
        }

        private void SimViewmodelOnGetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            RefreshListBox();
            TopUpViewer.Visibility = Visibility.Visible;
        }

        private void RefreshListBox()
        {
            TopUpViewer.ItemsSource = null;
            TopUpViewer.ItemsSource = App.Viewmodel.SimViewmodel.Topup;
        }
    }
}