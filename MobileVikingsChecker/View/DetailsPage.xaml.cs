using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tools.Annotations;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Parameter == null) 
                return;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            if (!await App.Viewmodel.DetailsViewmodel.GetUsage(App.Parameter, DateTime.Now.AddDays(-7), DateTime.Now)) 
                return;
            RefreshListBox();
            Viewer.Visibility = Visibility.Visible;
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.DetailsViewmodel.Usage;
        }
    }
}