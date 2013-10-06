using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Tools;
using VikingApi.ApiTools;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private bool _calendar;
        private bool _datepicker;

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
            _datepicker = true;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/check.png", "check", true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", "cancel", true, CalendarCancelOnClick));
            SubTitleBlock.Text = "pick a day";
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.DetailsViewmodel.CancelTask();
            Tools.Tools.SetProgressIndicator(false);
            Viewer.IsEnabled = true;
            Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
            ResetAppbar();
        }

        private async void CalendarCheckOnClick(object sender, EventArgs e)
        {
            if (_calendar)
                return;
            var date = DateTime.Now.AddDays(-1);
            try
            {
                date = DatePicker.SelectedDate;
            }
            catch (Exception)
            {
                Message.ShowToast("Looks like that's not a valid date, check again professor!");
                return;
            }
            if (date > DateTime.Now)
            {
                Message.ShowToast("Hey now, Marty McFly, let's stick to the present!");
                return;
            }
            _calendar = true;
            Viewer.IsEnabled = false;
            DatePicker.IsEnabled = false;
            App.Viewmodel.DetailsViewmodel.RenewToken();
            await App.Viewmodel.DetailsViewmodel.GetUsage(date, date.AddDays(1));
        }

        private void ResetAppbar()
        {
            BuildUsageAppbar();
            Viewer.Visibility = Visibility.Visible;
            DatePicker.Visibility = Visibility.Collapsed;
            _datepicker = false;
        }

        private void CalendarOnClick(object sender, EventArgs e)
        {
            Tools.Tools.SetProgressIndicator(false);
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

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_datepicker)
            {
                ResetAppbar();
                e.Cancel = true;
                base.OnBackKeyPress(e);
                Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
            }
            else
            {
                App.Viewmodel.DetailsViewmodel.CancelTask();
            }
        }

        private void DetailsViewmodel_GetInfoFinished(object sender, GetInfoCompletedArgs args)
        {
            if (args.Canceled)
                return;
            if (_calendar)
            {
                ResetAppbar();
            }
            _calendar = false;
            RefreshListBox();
            Viewer.IsEnabled = true;
            Viewer.Visibility = Visibility.Visible;
            Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.DetailsViewmodel.Usage;
        }
    }
}