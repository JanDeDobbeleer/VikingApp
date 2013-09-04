using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Fuel.Viewmodel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Fuel.View
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        private readonly DetailsViewmodel _viewmodel ;
        public DetailsPage()
        {
            InitializeComponent();
            _viewmodel = new DetailsViewmodel();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            string parameterValue = NavigationContext.QueryString["msisdn"];
            if (await _viewmodel.GetUsage(parameterValue))
                Viewer.Visibility = Visibility.Visible;
        }
    }
}