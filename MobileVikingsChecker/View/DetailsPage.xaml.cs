using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();
            DataContext = App.Viewmodel.DetailsViewmodel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Parameter == null) 
                return;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            if (await App.Viewmodel.DetailsViewmodel.GetUsage(App.Parameter))
                Viewer.Visibility = Visibility.Visible;
        }
    }
}