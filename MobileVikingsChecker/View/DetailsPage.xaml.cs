using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using Fuel.Localization.Resources;
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
            App.Viewmodel.UsageViewmodel.GetInfoFinished += DetailsViewmodel_GetInfoFinished;
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar { Mode = ApplicationBarMode.Default, Opacity = 1, IsVisible = true };
            BuildUsageAppbar();
        }

        private void BuildUsageAppbar()
        {
            ApplicationBar.Buttons.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/feature.calendar.png", AppResources.AppBarButtonCalendar, true, CalendarOnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuLastDay, true, LastOnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuLastWeek, true, LastOnClick));
            ApplicationBar.MenuItems.Add(Tools.Tools.CreateMenuItem(AppResources.AppBarMenuLastMonth, true, LastOnClick));
            SubTitleBlock.Text = AppResources.DetailsViewSubtitleDefault;
        }

        private async void LastOnClick(object sender, EventArgs e)
        {
            if ((sender as ApplicationBarMenuItem) == null)
                return;
            Viewer.IsEnabled = false;
            Tools.Tools.SetProgressIndicator(false);
            App.Viewmodel.UsageViewmodel.CancelTask();
            Viewer.Visibility = Visibility.Collapsed;
            App.Viewmodel.UsageViewmodel.RenewToken();
            DateTime date;
            if ((sender as ApplicationBarMenuItem).Text == AppResources.AppBarMenuLastDay)
            {
                date = DateTime.Today.AddDays(-1);
            }
            else if ((sender as ApplicationBarMenuItem).Text == AppResources.AppBarMenuLastWeek)
            {
                date = DateTime.Today.AddDays(-7);
            }
            else
            {
                date = DateTime.Today.AddDays(-30);
            }
            await App.Viewmodel.UsageViewmodel.GetUsage(date, DateTime.Now);
        }

        private void BuildCalendarAppbar()
        {
            _datepicker = true;
            ApplicationBar.Buttons.Clear();
            ApplicationBar.MenuItems.Clear();
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/arrow.png", AppResources.AppBarButtonNext, true, CalendarCheckOnClick));
            ApplicationBar.Buttons.Add(Tools.Tools.CreateButton("/Assets/cancel.png", AppResources.AppBarButtonCancel, true, CalendarCancelOnClick));
            SubTitleBlock.Text = AppResources.DetailsViewSubtitleFrom;
        }

        private void CalendarCancelOnClick(object sender, EventArgs e)
        {
            App.Viewmodel.UsageViewmodel.CancelTask();
            Tools.Tools.SetProgressIndicator(false);
            Viewer.IsEnabled = true;
            if (App.Viewmodel.UsageViewmodel.Usage != null)
                Viewer.ScrollIntoView(App.Viewmodel.UsageViewmodel.Usage.ElementAt(0));
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
                App.Viewmodel.UsageViewmodel.RenewToken();
                _isSecondDate = false;
                await App.Viewmodel.UsageViewmodel.GetUsage(_firstDate, _secondDate);
            }
            else
            {
                SubTitleBlock.Text = AppResources.DetailsViewSubtitleUntil;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/check.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.AppBarButtonOk;
                _isSecondDate = true;
            }
        }

        private bool CheckForValidDate()
        {
            try
            {
                if (DatePicker.SelectedDate > DateTime.Now)
                {
                    Message.ShowToast(AppResources.ToastFutureDate);
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
                        Message.ShowToast(AppResources.ToastInconsistentDateChoice);
                        return false;
                    }

                }
            }
            catch (Exception)
            {
                Message.ShowToast(AppResources.ToastInvalidDate);
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
            App.Viewmodel.UsageViewmodel.CancelTask();
            DatePicker.IsEnabled = true;
            BuildCalendarAppbar();
            Viewer.Visibility = Visibility.Collapsed;
            DatePicker.Visibility = Visibility.Visible;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SystemTray.ProgressIndicator = new ProgressIndicator();
            if (string.IsNullOrWhiteSpace(App.Viewmodel.UsageViewmodel.Msisdn))
                return;
            if (!Tools.Tools.HasInternetConnection())
            {
                Message.ShowToast(AppResources.ToastNoInternet);
            }
            else
            {
                await App.Viewmodel.UsageViewmodel.GetUsage(DateTime.Now.AddDays(-1), DateTime.Now);
            }
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (_datepicker && !_isSecondDate)
            {
                ResetAppbar();
                e.Cancel = true;
                base.OnBackKeyPress(e);
                if (App.Viewmodel.UsageViewmodel.Usage != null)
                    Viewer.ScrollIntoView(App.Viewmodel.UsageViewmodel.Usage.ElementAt(0));
            }
            else if (_isSecondDate)
            {
                SubTitleBlock.Text = AppResources.DetailsViewSubtitleFrom;
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).IconUri = new Uri("/Assets/arrow.png", UriKind.Relative);
                ((ApplicationBarIconButton)ApplicationBar.Buttons[0]).Text = AppResources.AppBarButtonNext;
                _isSecondDate = false;
                e.Cancel = true;
                base.OnBackKeyPress(e);
            }
            else
            {
                App.Viewmodel.UsageViewmodel.CancelTask();
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
            if (App.Viewmodel.UsageViewmodel.Usage == null)
            {
                Message.ShowToast(AppResources.ToastNoDataForPeriod);
                return;
            }
            Viewer.ScrollIntoView(App.Viewmodel.UsageViewmodel.Usage.ElementAt(0));
        }

        private void RefreshListBox()
        {
            Viewer.ItemsSource = null;
            Viewer.ItemsSource = App.Viewmodel.UsageViewmodel.Usage;
        }
    }
}