using System;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using VikingApi.ApiTools;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private bool _calendar;

        public DetailsPage()
        {
            InitializeComponent();
            BuildApplicationBar();
            App.Viewmodel.DetailsViewmodel.GetInfoFinished += DetailsViewmodel_GetInfoFinished;
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
            SubTitleBlock.Text = "usage";
        }

        private void BuildCalendarAppbar()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/check.png", "check", true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", "cancel", true, CalendarCancelOnClick));
            SubTitleBlock.Text = "pick a day";
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.DetailsViewmodel.CancelTask();
            ResetAppbar();
        }

        private async void CalendarCheckOnClick(object sender, EventArgs e)
        {
            if(_calendar)
                return;
            _calendar = true;
            Viewer.IsEnabled = false;
            var date = DatePicker.SelectedDate;
            DatePicker.IsEnabled = false;
            App.Viewmodel.DetailsViewmodel.RenewToken();
            await App.Viewmodel.DetailsViewmodel.GetUsage(date, date.AddDays(1));
        }

        private void ResetAppbar()
        {
            BuildUsageAppbar();
            Viewer.Visibility = Visibility.Visible;
            DatePicker.Visibility = Visibility.Collapsed;
        }

        private void CalendarOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.DetailsViewmodel.CancelTask();
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
            await App.Viewmodel.DetailsViewmodel.GetUsage(DateTime.Now.AddDays(-1), DateTime.Now);
        }

        private void DetailsViewmodel_GetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled) 
                return;
            if (_calendar)
            {
                ResetAppbar();
                _calendar = false;
            }
            RefreshListBox();
            Viewer.IsEnabled = true;
            Viewer.Visibility = Visibility.Visible;
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.DetailsViewmodel.Usage;
        }
    }
}