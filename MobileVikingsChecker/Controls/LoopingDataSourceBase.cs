using System;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Fuel.Controls
{
    public abstract class LoopingDataSourceBase : ILoopingSelectorDataSource
    {
        private object _selectedItem;
        public bool IsEmpty { get; private set; }

        #region ILoopingSelectorDataSource Members

        public abstract object GetNext(object relativeTo);

        public abstract object GetPrevious(object relativeTo);

        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                // save the previously selected item so that we can use it 
                // to construct the event arguments for the SelectionChanged event
                var previousSelectedItem = _selectedItem;
                // this will use the Equals method if it is overridden for the data source item class
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;
                    // fire the SelectionChanged event
                    OnSelectionChanged(previousSelectedItem, _selectedItem);
                    OnSelectionMoved(previousSelectedItem, _selectedItem);
                }
                else
                {
                    OnSelectionMoved(previousSelectedItem, _selectedItem);
                }
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<SelectionChangedEventArgs> SelectionMoved;

        protected virtual void OnSelectionChanged(object oldSelectedItem, object newSelectedItem)
        {
            var handler = SelectionChanged;
            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs(new[] { oldSelectedItem }, new[] { newSelectedItem }));
            }
        }

        protected virtual void OnSelectionMoved(object oldSelectedItem, object newSelectedItem)
        {
            var handler = SelectionMoved;
            if (handler != null)
            {
                handler(this, new SelectionChangedEventArgs(new[] { oldSelectedItem }, new[] { newSelectedItem }));
            }
        }

        #endregion
    }
}
