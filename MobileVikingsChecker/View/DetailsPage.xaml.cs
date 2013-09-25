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
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/feature.calendar.png", "refresh", true, CalendarOnClick));
        }

        private void BuildCalendarAppbar()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/check.png", "refresh", true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", "refresh", true, CalendarCancelOnClick));
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            ResetAppbar();
        }

        private void CalendarCheckOnClick(object sender, EventArgs e)
        {
            ResetAppbar();
            //TODO: fix date not formatting correctly in datepicker.xaml.cs
            var date = DatePicker.SelectedDate;
        }

        private void ResetAppbar()
        {
            BuildUsageAppbar();
            Viewer.Visibility = Visibility.Visible;
            DatePicker.Visibility = Visibility.Collapsed;
        }

        private void CalendarOnClick(object sender, EventArgs e)
        {
            BuildCalendarAppbar();
            Viewer.Visibility = Visibility.Collapsed;
            DatePicker.Visibility = Visibility.Visible;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.Parameter == null)
                return;
            SystemTray.ProgressIndicator = new ProgressIndicator();
            if (!await App.Viewmodel.DetailsViewmodel.GetUsage(new[] { ValuePair(App.Parameter, Usage.Msisdn), ValuePair(DateTime.Now.AddDays(-1).ToVikingApiTimeFormat(), Usage.FromDate), ValuePair("50", Usage.PageSize), ValuePair(DateTime.Now.ToVikingApiTimeFormat(), Usage.UntilDate)}))
                return;
            RefreshListBox();
            Viewer.Visibility = Visibility.Visible;
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.DetailsViewmodel.Usage;
        }

        private KeyValuePair ValuePair(object content, string name)
        {
            return new KeyValuePair {content = content, name = name};
        }
    }
}