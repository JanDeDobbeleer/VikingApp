﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// An infinitely scrolling, UI- and data-virtualizing selection control.
    /// </summary>
    [TemplatePart(Name = ItemsPanelName, Type = typeof(Panel))]
    [TemplatePart(Name = CenteringTransformName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = PanningTransformName, Type = typeof(TranslateTransform))]
    public class LoopingSelector : Control
    {
        // The names of the template parts
        private const string ItemsPanelName = "ItemsPanel";
        private const string CenteringTransformName = "CenteringTransform";
        private const string PanningTransformName = "PanningTransform";

        private static readonly Duration _selectDuration = new Duration(TimeSpan.FromMilliseconds(500));
        private readonly IEasingFunction _selectEase = new ExponentialEase() { EasingMode = EasingMode.EaseInOut };

        private static readonly Duration _panDuration = new Duration(TimeSpan.FromMilliseconds(100));
        private readonly IEasingFunction _panEase = new ExponentialEase();

        private DoubleAnimation _panelAnimation;
        private Storyboard _panelStoryboard;

        private Panel _itemsPanel;
        private TranslateTransform _panningTransform;
        private TranslateTransform _centeringTransform;

        private bool _isSelecting;
        private LoopingSelectorItem _selectedItem;

        private Queue<LoopingSelectorItem> _temporaryItemsPool;

        private double _minimumPanelScroll = float.MinValue;
        private double _maximumPanelScroll = float.MaxValue;

        private int _additionalItemsCount = 0;

        private bool _isAnimating;

        private enum State
        {
            Normal,
            Expanded,
            Dragging,
            Snapping,
            Flicking
        }

        private State _state;

        /// <summary>
        /// The data source that the this control is the view for.
        /// </summary>
        public ILoopingSelectorDataSource DataSource
        {
            get { return (ILoopingSelectorDataSource)GetValue(DataSourceProperty); }
            set
            {
                if (DataSource != null)
                {
                    DataSource.SelectionChanged -= value_SelectionChanged;
                }

                SetValue(DataSourceProperty, value);

                if (value != null)
                {
                    value.SelectionChanged += value_SelectionChanged;
                }
            }
        }

        void value_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady)
            {
                return;
            }

            if (!_isSelecting && e.AddedItems.Count == 1)
            {
                object selection = e.AddedItems[0];
                //Debug.WriteLine("Selecting {0}", selection);
                foreach (LoopingSelectorItem child in _itemsPanel.Children)
                {
                    if (child.DataContext == selection)
                    {
                        SelectAndSnapTo(child);
                        return;
                    }
                }
                UpdateData();
            }
        }

        /// <summary>
        /// The DataSource DependencyProperty
        /// </summary>
        public static readonly DependencyProperty DataSourceProperty =
            DependencyProperty.Register("DataSource", typeof(ILoopingSelectorDataSource), typeof(LoopingSelector), new PropertyMetadata(null, OnDataModelChanged));

        private static void OnDataModelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            LoopingSelector picker = (LoopingSelector)obj;
            picker.UpdateData();
        }

        void DataModel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsReady)
            {
                return;
            }

            if (!_isSelecting && e.AddedItems.Count == 1)
            {
                object selection = e.AddedItems[0];
                //Debug.WriteLine("Selecting {0}", selection);
                foreach (LoopingSelectorItem child in _itemsPanel.Children)
                {
                    if (child.DataContext == selection)
                    {
                        SelectAndSnapTo(child);
                        break;
                    }
                }

                UpdateData();
            }
        }

        /// <summary>
        /// The ItemTemplate property
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// The ItemTemplate DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(LoopingSelector), new PropertyMetadata(null));

        /// <summary>
        /// The size of the items, excluding the ItemMargin.
        /// </summary>
        public Size ItemSize { get; set; }

        /// <summary>
        /// The margin around the items, to be a part of the touchable area.
        /// </summary>
        public Thickness ItemMargin { get; set; }


        public Style ItemStyle
        {
            get { return (Style)GetValue(ItemStyleProperty); }
            set { SetValue(ItemStyleProperty, value); }
        }

        public static readonly DependencyProperty ItemStyleProperty =
            DependencyProperty.Register("ItemStyle", typeof(Style), typeof(LoopingSelector), new PropertyMetadata(null));

        /// <summary>
        /// Creates a new LoopingSelector.
        /// </summary>
        public LoopingSelector()
        {
            DefaultStyleKey = typeof(LoopingSelector);
            CreateEventHandlers();
        }

        /// <summary>
        /// The IsExpanded property controls the expanded state of this control.
        /// </summary>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// The IsExpanded DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(LoopingSelector), new PropertyMetadata(false, OnIsExpandedChanged));

        /// <summary>
        /// The IsExpandedChanged event will be raised whenever the IsExpanded state changes.
        /// </summary>
        public event DependencyPropertyChangedEventHandler IsExpandedChanged;

        private static void OnIsExpandedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            LoopingSelector picker = (LoopingSelector)sender;

            picker.UpdateItemState();
            if (!picker.IsExpanded)
            {
                picker.SelectAndSnapToClosest();
            }

            if (picker._state == State.Normal || picker._state == State.Expanded)
            {
                picker._state = picker.IsExpanded ? State.Expanded : State.Normal;
            }

            var listeners = picker.IsExpandedChanged;
            if (listeners != null)
            {
                listeners(picker, e);
            }
        }

        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes (such as a rebuilding layout pass) call <see cref="M:FrameworkElement.ApplyTemplate()"/>.
        /// In simplest terms, this means the method is called just before a UI element displays in an application.
        /// For more information, see Remarks.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Find the template parts. Create dummy objects if parts are missing to avoid
            // null checks throughout the code (although we can't escape them completely.)
            _itemsPanel = GetTemplateChild(ItemsPanelName) as Panel ?? new Canvas();
            _centeringTransform = GetTemplateChild(CenteringTransformName) as TranslateTransform ?? new TranslateTransform();
            _panningTransform = GetTemplateChild(PanningTransformName) as TranslateTransform ?? new TranslateTransform();

            CreateVisuals();
        }


        void LoopingSelector_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("MouseLeftButtonDown {0}", sender);
            if (_isAnimating)
            {
                double y = _panningTransform.Y;
                StopAnimation();
                _panningTransform.Y = y;
                _isAnimating = false;
                _state = State.Dragging;
            }
        }

        void LoopingSelector_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Debug.WriteLine("MouseLeftButtonUp {0}", sender);
            if (_selectedItem != sender && _state == State.Dragging && !_isAnimating)
            {
                SelectAndSnapToClosest();
                _state = State.Expanded;
            }
        }

        void listener_Tap(object sender, GestureEventArgs e)
        {
            //Debug.WriteLine("listener_Tap");

            if (_panningTransform != null)
            {
                SelectAndSnapToClosest();
                e.Handled = true;
            }
        }
        double _dragTarget;

        void listener_DragStarted(object sender, DragStartedGestureEventArgs e)
        {
            //Debug.WriteLine("listener_DragStarted");

            if (e.Direction == Orientation.Vertical)
            {
                _state = State.Dragging;
                e.Handled = true;
                _selectedItem = null;
                if (!IsExpanded)
                {
                    IsExpanded = true;
                }
                _dragTarget = _panningTransform.Y;
                UpdateItemState();
            }
        }

        void listener_DragDelta(object sender, DragDeltaGestureEventArgs e)
        {
            //Debug.WriteLine("listener_DragDelta");

            if (e.Direction == Orientation.Vertical && _state == State.Dragging)
            {
                AnimatePanel(_panDuration, _panEase, _dragTarget += e.VerticalChange);
                e.Handled = true;
            }
        }

        void listener_DragCompleted(object sender, DragCompletedGestureEventArgs e)
        {
            //Debug.WriteLine("listener_DragCompleted");
            if (_state == State.Dragging)
            {
                SelectAndSnapToClosest();
            }
            _state = State.Expanded;
        }

        void listener_Flick(object sender, FlickGestureEventArgs e)
        {
            //Debug.WriteLine("listener_Flick");

            if (e.Direction == Orientation.Vertical)
            {
                _state = State.Flicking;

                _selectedItem = null;
                if (!IsExpanded)
                {
                    IsExpanded = true;
                }

                Point velocity = new Point(0, e.VerticalVelocity);
                double flickDuration = PhysicsConstants.GetStopTime(velocity);
                Point flickEndPoint = PhysicsConstants.GetStopPoint(velocity);
                IEasingFunction flickEase = PhysicsConstants.GetEasingFunction(flickDuration);

                AnimatePanel(new Duration(TimeSpan.FromSeconds(flickDuration)), flickEase, _panningTransform.Y + flickEndPoint.Y);

                e.Handled = true;

                _selectedItem = null;
                UpdateItemState();
            }
        }

        void LoopingSelector_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _centeringTransform.Y = Math.Round(e.NewSize.Height / 2);
            Clip = new RectangleGeometry() { Rect = new Rect(0, 0, e.NewSize.Width, e.NewSize.Height) };
            UpdateData();
        }

        void wrapper_Click(object sender, EventArgs e)
        {
            //Debug.WriteLine("wrapper_Click");

            if (_state == State.Normal)
            {
                _state = State.Expanded;
                IsExpanded = true;
            }
            else if (_state == State.Expanded)
            {
                if (!_isAnimating && sender == _selectedItem)
                {
                    _state = State.Normal;
                    IsExpanded = false;
                }
                else if (sender != _selectedItem && !_isAnimating)
                {
                    //Debug.WriteLine("Selecting from wrapper_Click {0}", sender);
                    SelectAndSnapTo((LoopingSelectorItem)sender);
                }
            }
        }

        private void SelectAndSnapTo(LoopingSelectorItem item)
        {
            if (item == null)
            {
                return;
            }

            if (_selectedItem != null)
            {
                _selectedItem.SetState(IsExpanded ? LoopingSelectorItem.State.Expanded : LoopingSelectorItem.State.Normal, true);
            }

            if (_selectedItem != item)
            {
                _selectedItem = item;
                // Update DataSource.SelectedItem aynchronously so that animations have a chance to start.
                Dispatcher.BeginInvoke(() =>
                {
                    _isSelecting = true;
                    DataSource.SelectedItem = item.DataContext;
                    _isSelecting = false;
                });
            }

            _selectedItem.SetState(LoopingSelectorItem.State.Selected, true);

            TranslateTransform transform = item.Transform;
            if (transform != null)
            {
                double newPosition = -transform.Y - Math.Round(item.ActualHeight / 2);
                if (_panningTransform.Y != newPosition)
                {
                    AnimatePanel(_selectDuration, _selectEase, newPosition);
                }
            }
        }

        private void UpdateData()
        {
            if (!IsReady)
            {
                return;
            }

            // Save all items
            _temporaryItemsPool = new Queue<LoopingSelectorItem>(_itemsPanel.Children.Count);
            foreach (LoopingSelectorItem item in _itemsPanel.Children)
            {
                if (item.GetState() == LoopingSelectorItem.State.Selected)
                {
                    item.SetState(LoopingSelectorItem.State.Normal, false);
                }
                _temporaryItemsPool.Enqueue(item);
                item.Remove();
            }

            _itemsPanel.Children.Clear();
            StopAnimation();
            _panningTransform.Y = 0;

            // Reset the extents
            _minimumPanelScroll = float.MinValue;
            _maximumPanelScroll = float.MaxValue;

            Balance();
        }

        private void AnimatePanel(Duration duration, IEasingFunction ease, double to)
        {
            // Be sure not to run past the first or last items
            double newTo = Math.Max(_minimumPanelScroll, Math.Min(_maximumPanelScroll, to));
            if (to != newTo)
            {
                // Adjust the duration
                double originalDelta = Math.Abs(_panningTransform.Y - to);
                double modifiedDelta = Math.Abs(_panningTransform.Y - newTo);
                double factor = modifiedDelta / originalDelta;

                duration = new Duration(TimeSpan.FromMilliseconds(duration.TimeSpan.Milliseconds * factor));

                to = newTo;
            }

            double from = _panningTransform.Y;
            StopAnimation();
            CompositionTarget.Rendering += AnimationPerFrameCallback;

            _panelAnimation.Duration = duration;
            _panelAnimation.EasingFunction = ease;
            _panelAnimation.From = from;
            _panelAnimation.To = to;
            _panelStoryboard.Begin();
            _panelStoryboard.SeekAlignedToLastTick(TimeSpan.Zero);

            _isAnimating = true;
        }

        private void StopAnimation()
        {
            _panelStoryboard.Stop();
            CompositionTarget.Rendering -= AnimationPerFrameCallback;
        }

        private void Brake(double newStoppingPoint)
        {
            double originalDelta = _panelAnimation.To.Value - _panelAnimation.From.Value;
            double remainingDelta = newStoppingPoint - _panningTransform.Y;
            double factor = remainingDelta / originalDelta;

            Duration duration = new Duration(TimeSpan.FromMilliseconds(_panelAnimation.Duration.TimeSpan.Milliseconds * factor));

            AnimatePanel(duration, _panelAnimation.EasingFunction, newStoppingPoint);
        }

        private bool IsReady
        {
            get { return ActualHeight > 0 && DataSource != null && _itemsPanel != null; }
        }

        /// <summary>
        /// Balances the items.
        /// </summary>
        private void Balance()
        {
            if (!IsReady)
            {
                return;
            }

            double actualItemWidth = ActualItemWidth;
            double actualItemHeight = ActualItemHeight;

            _additionalItemsCount = (int)Math.Round((ActualHeight * 1.5) / actualItemHeight);

            LoopingSelectorItem closestToMiddle = null;
            int closestToMiddleIndex = -1;

            if (_itemsPanel.Children.Count == 0)
            {
                // We need to get the selection and start from there
                closestToMiddleIndex = 0;
                _selectedItem = closestToMiddle = CreateAndAddItem(_itemsPanel, DataSource.SelectedItem);
                closestToMiddle.Transform.Y = -actualItemHeight / 2;
                closestToMiddle.Transform.X = (ActualWidth - actualItemWidth) / 2;
                closestToMiddle.SetState(LoopingSelectorItem.State.Selected, false);
            }
            else
            {
                closestToMiddleIndex = GetClosestItem();
                closestToMiddle = (LoopingSelectorItem)_itemsPanel.Children[closestToMiddleIndex];
            }

            int itemsBeforeCount;
            LoopingSelectorItem firstItem = GetFirstItem(closestToMiddle, out itemsBeforeCount);

            int itemsAfterCount;
            LoopingSelectorItem lastItem = GetLastItem(closestToMiddle, out itemsAfterCount);

            // Does the top need items?
            if (itemsBeforeCount < itemsAfterCount || itemsBeforeCount < _additionalItemsCount)
            {
                while (itemsBeforeCount < _additionalItemsCount)
                {
                    object newData = DataSource.GetPrevious(firstItem.DataContext);
                    if (newData == null)
                    {
                        // There may be room to display more items, but there is no more data.
                        _maximumPanelScroll = -firstItem.Transform.Y - actualItemHeight / 2;
                        if (_isAnimating && _panelAnimation.To.Value > _maximumPanelScroll)
                        {
                            Brake(_maximumPanelScroll);
                        }
                        break;
                    }

                    LoopingSelectorItem newItem = null;

                    // Can an item from the bottom be re-used?
                    if (itemsAfterCount > _additionalItemsCount)
                    {
                        newItem = lastItem;
                        lastItem = lastItem.Previous;
                        newItem.Remove();
                        newItem.Content = newItem.DataContext = newData;
                    }
                    else
                    {
                        // Make a new item
                        newItem = CreateAndAddItem(_itemsPanel, newData);
                        newItem.Transform.X = (ActualWidth - actualItemWidth) / 2;
                    }

                    // Put the new item on the top
                    newItem.Transform.Y = firstItem.Transform.Y - actualItemHeight;
                    newItem.InsertBefore(firstItem);
                    firstItem = newItem;

                    ++itemsBeforeCount;
                }
            }

            // Does the bottom need items?
            if (itemsAfterCount < itemsBeforeCount || itemsAfterCount < _additionalItemsCount)
            {
                while (itemsAfterCount < _additionalItemsCount)
                {
                    object newData = DataSource.GetNext(lastItem.DataContext);
                    if (newData == null)
                    {
                        // There may be room to display more items, but there is no more data.
                        _minimumPanelScroll = -lastItem.Transform.Y - actualItemHeight / 2;
                        if (_isAnimating && _panelAnimation.To.Value < _minimumPanelScroll)
                        {
                            Brake(_minimumPanelScroll);
                        }
                        break;
                    }

                    LoopingSelectorItem newItem = null;

                    // Can an item from the top be re-used?
                    if (itemsBeforeCount > _additionalItemsCount)
                    {
                        newItem = firstItem;
                        firstItem = firstItem.Next;
                        newItem.Remove();
                        newItem.Content = newItem.DataContext = newData;
                    }
                    else
                    {
                        // Make a new item
                        newItem = CreateAndAddItem(_itemsPanel, newData);
                        newItem.Transform.X = (ActualWidth - actualItemWidth) / 2;
                    }

                    // Put the new item on the bottom
                    newItem.Transform.Y = lastItem.Transform.Y + actualItemHeight;
                    newItem.InsertAfter(lastItem);
                    lastItem = newItem;

                    ++itemsAfterCount;
                }
            }

            _temporaryItemsPool = null;
        }

        private static LoopingSelectorItem GetFirstItem(LoopingSelectorItem item, out int count)
        {
            count = 0;
            while (item.Previous != null)
            {
                ++count;
                item = item.Previous;
            }

            return item;
        }

        private static LoopingSelectorItem GetLastItem(LoopingSelectorItem item, out int count)
        {
            count = 0;
            while (item.Next != null)
            {
                ++count;
                item = item.Next;
            }

            return item;
        }

        void AnimationPerFrameCallback(object sender, EventArgs e)
        {
            Balance();
        }

        private int GetClosestItem()
        {
            if (!IsReady)
            {
                return -1;
            }

            double actualItemHeight = ActualItemHeight;

            int count = _itemsPanel.Children.Count;
            double panelY = _panningTransform.Y;
            double halfHeight = actualItemHeight / 2;
            int found = -1;
            double closestDistance = double.MaxValue;

            for (int index = 0; index < count; ++index)
            {
                LoopingSelectorItem wrapper = (LoopingSelectorItem)_itemsPanel.Children[index];
                double distance = Math.Abs((wrapper.Transform.Y + halfHeight) + panelY);
                if (distance <= halfHeight)
                {
                    found = index;
                    break;
                }
                else if (closestDistance > distance)
                {
                    closestDistance = distance;
                    found = index;
                }
            }

            return found;
        }

        void PanelStoryboardCompleted(object sender, EventArgs e)
        {
            CompositionTarget.Rendering -= AnimationPerFrameCallback;
            _isAnimating = false;
            if (_state != State.Dragging)
            {
                SelectAndSnapToClosest();
            }
        }

        private void SelectAndSnapToClosest()
        {
            if (!IsReady)
            {
                return;
            }

            int index = GetClosestItem();
            if (index == -1)
            {
                return;
            }

            LoopingSelectorItem item = (LoopingSelectorItem)_itemsPanel.Children[index];
            //Debug.WriteLine("Selecting from SelectAndSnapToClosest {0}", item.DataContext);
            SelectAndSnapTo(item);
        }

        private void UpdateItemState()
        {
            if (!IsReady)
            {
                return;
            }

            bool isExpanded = IsExpanded;

            foreach (LoopingSelectorItem child in _itemsPanel.Children)
            {
                if (child == _selectedItem)
                {
                    child.SetState(LoopingSelectorItem.State.Selected, true);
                }
                else
                {
                    child.SetState(isExpanded ? LoopingSelectorItem.State.Expanded : LoopingSelectorItem.State.Normal, true);
                }
            }
        }

        private double ActualItemWidth { get { return Padding.Left + Padding.Right + ItemSize.Width; } }
        private double ActualItemHeight { get { return Padding.Top + Padding.Bottom + ItemSize.Height; } }

        private void CreateVisuals()
        {
            _panelAnimation = new DoubleAnimation();
            Storyboard.SetTarget(_panelAnimation, _panningTransform);
            Storyboard.SetTargetProperty(_panelAnimation, new PropertyPath("Y"));

            _panelStoryboard = new Storyboard();
            _panelStoryboard.Children.Add(_panelAnimation);
            _panelStoryboard.Completed += PanelStoryboardCompleted;
        }

        private void CreateEventHandlers()
        {

            SizeChanged += new SizeChangedEventHandler(LoopingSelector_SizeChanged);

            var listener = GestureService.GetGestureListener(this);
            listener.DragStarted += new EventHandler<DragStartedGestureEventArgs>(listener_DragStarted);
            listener.DragDelta += new EventHandler<DragDeltaGestureEventArgs>(listener_DragDelta);
            listener.DragCompleted += new EventHandler<DragCompletedGestureEventArgs>(listener_DragCompleted);
            listener.Flick += new EventHandler<FlickGestureEventArgs>(listener_Flick);
            listener.Tap += new EventHandler<GestureEventArgs>(listener_Tap);

            AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(LoopingSelector_MouseLeftButtonDown), true);
            AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler(LoopingSelector_MouseLeftButtonUp), true);
        }

        private LoopingSelectorItem CreateAndAddItem(Panel parent, object content)
        {
            bool reuse = _temporaryItemsPool != null && _temporaryItemsPool.Count > 0;

            LoopingSelectorItem wrapper = reuse ? _temporaryItemsPool.Dequeue() : new LoopingSelectorItem();

            if (!reuse)
            {
                wrapper.ContentTemplate = this.ItemTemplate;
                wrapper.Width = ItemSize.Width;
                wrapper.Height = ItemSize.Height;
                wrapper.Padding = ItemMargin;

                wrapper.Click += new EventHandler<EventArgs>(wrapper_Click);

                if (ItemStyle != null) { wrapper.Style = ItemStyle; } //custom style for item

            }

            wrapper.DataContext = wrapper.Content = content;

            parent.Children.Add(wrapper); // Need to do this before calling ApplyTemplate
            if (!reuse)
            {
                wrapper.ApplyTemplate();
            }

            return wrapper;
        }
    }
}
