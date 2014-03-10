using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Fuel.Controls
{
    public partial class DatePicker: UserControl
    {
        private ListLoopingDataSource<int> _dayList;
        private ListLoopingDataSource<int> _monthList;
        private ListLoopingDataSource<int> _yearList;

        public DateTime SelectedDate
        {
            get
            {
                return new DateTime((int)_yearList.SelectedItem, (int)_monthList.SelectedItem, (int)_dayList.SelectedItem);
            }
        }

        public DatePicker()
        {
            InitializeComponent();
            LoadLists();
        }

        private void LoadLists()
        {
            _dayList = new ListLoopingDataSource<int> { Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, SelectedItem = DateTime.Now.Day, Tag = "day"};
            _monthList = new ListLoopingDataSource<int> { Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, SelectedItem = DateTime.Now.Month, Tag = "month"};
            var years = new List<int>();
            for (var i = 2009; i <= DateTime.Now.Year; i++)
            {
                years.Add(i);
            }
            _yearList = new ListLoopingDataSource<int> { Items = years, SelectedItem = DateTime.Now.Year, Tag = "year"};

            SelectorDay.DataSource = _dayList;
            SelectorMonth.DataSource = _monthList;
            SelectorYear.DataSource = _yearList;

            _dayList.SelectionMoved += List_SelectionMoved;
            _monthList.SelectionMoved += List_SelectionMoved;
            _yearList.SelectionMoved += List_SelectionMoved;
        }

        private void List_SelectionMoved(object sender, SelectionChangedEventArgs e)
        {
            if (sender as ListLoopingDataSource<int> == null) 
                return;
            switch ((sender as ListLoopingDataSource<int>).Tag)
            {
                case "day":
                    SelectorDay.IsExpanded = false;
                    break;
                case "month":
                    SelectorMonth.IsExpanded = false;
                    break;
                case "year":
                    SelectorYear.IsExpanded = false;
                    break;
            }
        }
    }
}
