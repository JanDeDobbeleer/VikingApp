using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tools;
using Tools.Annotations;
using VikingApi.ApiTools;

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
            if (!await App.Viewmodel.DetailsViewmodel.GetUsage(new[] { new KeyValuePair { content = App.Parameter, name = "msisdn" }, new KeyValuePair { content = DateTime.Now.AddDays(-5).ToVikingApiTimeFormat(), name = "from_date" }, new KeyValuePair { content = "1000", name = "page_size" }, new KeyValuePair { name = "page", content = "0"} })) 
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