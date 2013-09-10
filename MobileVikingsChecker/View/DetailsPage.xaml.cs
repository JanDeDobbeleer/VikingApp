using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

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
            var parameterValue = NavigationContext.QueryString["msisdn"];
            if (await App.Viewmodel.DetailsViewmodel.GetUsage(parameterValue))
                Viewer.Visibility = Visibility.Visible;
        }
    }
}