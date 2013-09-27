using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tools;
using VikingApi.ApiTools;
using VikingApi.Json;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        public DetailsPage()
        {
            InitializeComponent();
            BuildApplicationBar();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            BuildUsageAppbar();
        }

        private void BuildUsageAppbar()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/feature.calendar.png", "calendar", true, CalendarOnClick));
        }

        private void BuildCalendarAppbar()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/check.png", "check", true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", "cancel", true, CalendarCancelOnClick));
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            ResetAppbar();
        }

        private async void CalendarCheckOnClick(object sender, EventArgs e)
        {
            Viewer.IsEnabled = false;
            var date = DatePicker.SelectedDate;
            if (!await App.Viewmodel.DetailsViewmodel.GetUsage(date, date.AddDays(1)))
                return;
            DatePicker.IsEnabled = false;
            RefreshListBox();
            ResetAppbar();
        }

        private void ResetAppbar()
        {
            BuildUsageAppbar();
            Viewer.Visibility = Visibility.Visible;
            DatePicker.Visibility = Visibility.Collapsed;
        }

        private void CalendarOnClick(object sender, EventArgs e)
        {
            DatePicker.IsEnabled = true;
            BuildCalendarAppbar();
            Viewer.Visibility = Visibility.Collapsed;
            DatePicker.Visibility = Visibility.Visible;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(App.Viewmodel.DetailsViewmodel.Msisdn))
                return;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            if (!await App.Viewmodel.DetailsViewmodel.GetUsage(DateTime.Now.AddDays(-1), DateTime.Now))
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