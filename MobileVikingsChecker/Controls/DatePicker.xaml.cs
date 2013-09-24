using System;
using System.Collections.Generic;

namespace Fuel.Controls
{
    public partial class DatePicker
    {
        public DateTime SelectedDate
        {
            get
            {
                return new DateTime((int)((ListLoopingDataSource<int>)SelectorYear.DataSource).SelectedItem, (int)((ListLoopingDataSource<int>)SelectorMonth.DataSource).SelectedItem, (int)((ListLoopingDataSource<int>)SelectorYear.DataSource).SelectedItem);
            }
        }

        public DatePicker()
        {
            InitializeComponent();
            LoadLists();
        }

        private void LoadLists()
        {
            SelectorDay.DataSource = new ListLoopingDataSource<int> { Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31 }, SelectedItem = DateTime.Now.Day };
            SelectorMonth.DataSource = new ListLoopingDataSource<int> { Items = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, SelectedItem = DateTime.Now.Month };
            var years = new List<int>();
            for (var i = 2009; i <= DateTime.Now.Year; i++)
            {
                years.Add(i);
            }
            SelectorYear.DataSource = new ListLoopingDataSource<int> { Items = years, SelectedItem = DateTime.Now.Year };
        }
    }
}
