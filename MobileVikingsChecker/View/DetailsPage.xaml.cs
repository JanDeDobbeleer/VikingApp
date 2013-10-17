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
        private bool _isSecondDate;

        private DateTime _firstDate;
        private DateTime _secondDate;

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
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("last day", true, LastOnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("last week", true, LastOnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem("last month", true, LastOnClick));
            SubTitleBlock.Text = "usage";
        }

        private async void LastOnClick(object sender, EventArgs e)
        {
            if ((sender as ApplicationBarMenuItem) == null)
                return;
            Viewer.IsEnabled = false;
            Tools.Tools.SetProgressIndicator(false);
            App.Viewmodel.DetailsViewmodel.CancelTask();
            Viewer.Visibility = Visibility.Collapsed;
            App.Viewmodel.DetailsViewmodel.RenewToken();
            switch ((sender as ApplicationBarMenuItem).Text)
            {
                case "last day":
                    await App.Viewmodel.DetailsViewmodel.GetUsage(DateTime.Today.AddDays(-1), DateTime.Now);
                    break;
                case "last week":
                    await App.Viewmodel.DetailsViewmodel.GetUsage(DateTime.Today.AddDays(-7), DateTime.Now);
                    break;
                case "last month":
                    await App.Viewmodel.DetailsViewmodel.GetUsage(DateTime.Today.AddDays(-30), DateTime.Now);
                    break;
            }
        }

        private void BuildCalendarAppbar()
        {
            _datepicker = true;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/arrow.png", "next", true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", "cancel", true, CalendarCancelOnClick));
            SubTitleBlock.Text = "from";
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.DetailsViewmodel.CancelTask();
            Tools.Tools.SetProgressIndicator(false);
            Viewer.IsEnabled = true;
            if (App.Viewmodel.DetailsViewmodel.Usage != null)
                Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
            ResetAppbar();
        }

        private async void CalendarCheckOnClick(object sender, EventArgs e)
        {
            if (_calendar)
                return;
            if (!CheckForValidDate())
                return;
            if (_isSecondDate)
            {
                _calendar = true;
                Viewer.IsEnabled = false;
                DatePicker.IsEnabled = false;
                App.Viewmodel.DetailsViewmodel.RenewToken();
                _isSecondDate = false;
                await App.Viewmodel.DetailsViewmodel.GetUsage(_firstDate, _secondDate);
            }
            else
            {
                SubTitleBlock.Text = "until";
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/check.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = "ok";
                _isSecondDate = true;
            }
        }

        private bool CheckForValidDate()
        {
            try
            {
                if (DatePicker.SelectedDate > DateTime.Now)
                {
                    Message.ShowToast("Hey now, Marty McFly, let's stick to the present!");
                    return false;
                }
                if (!_isSecondDate)
                {
                    _firstDate = DatePicker.SelectedDate;
                }
                else
                {
                    _secondDate = (DatePicker.SelectedDate.Date == DateTime.Now.Date) ? DateTime.Now : DatePicker.SelectedDate;
                    if (_secondDate < _firstDate)
                    {
                        Message.ShowToast("Please select a date later than the first one");
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                Message.ShowToast("Looks like that's not a valid date, check again professor!");
                return false;
            }
            return true;
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
            if (_datepicker && !_isSecondDate)
            {
                ResetAppbar();
                e.Cancel = true;
                base.OnBackKeyPress(e);
                if (App.Viewmodel.DetailsViewmodel.Usage != null)
                    Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
            }
            else if (_isSecondDate)
            {
                SubTitleBlock.Text = "from";
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/arrow.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = "next";
                _isSecondDate = false;
                e.Cancel = true;
                base.OnBackKeyPress(e);
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
            if (App.Viewmodel.DetailsViewmodel.Usage == null)
            {
                Message.ShowToast("There is no data for this period");
                return;
            }
            Viewer.ScrollIntoView(App.Viewmodel.DetailsViewmodel.Usage.ElementAt(0));
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.DetailsViewmodel.Usage;
        }
    }
}