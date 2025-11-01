using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DynamicUniform
{
    /// <summary>
    /// 虚拟化的UniformGrid
    /// </summary>
    public class VirtualizingUniformGrid : VirtualizingPanel, IScrollInfo
    {
        private readonly TranslateTransform _trans = new TranslateTransform();
        public VirtualizingUniformGrid()
        {
            this.RenderTransform = _trans;
        }

        #region DependencyProperties
        public static readonly DependencyProperty ChildWidthProperty = DependencyProperty.RegisterAttached("ChildWidth", typeof(double), typeof(VirtualizingUniformGrid), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ChildHeightProperty = DependencyProperty.RegisterAttached("ChildHeight", typeof(double), typeof(VirtualizingUniformGrid), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        //鼠标每一次滚动 UI上的偏移
        public static readonly DependencyProperty ScrollOffsetProperty = DependencyProperty.RegisterAttached("ScrollOffset", typeof(int), typeof(VirtualizingUniformGrid), new PropertyMetadata(10));

        //列数
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached("Columns", typeof(int), typeof(VirtualizingUniformGrid), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        //行数
        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached("Rows", typeof(int), typeof(VirtualizingUniformGrid), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public int ScrollOffset
        {
            get => Convert.ToInt32(GetValue(ScrollOffsetProperty));
            set => SetValue(ScrollOffsetProperty, value);
        }

        public double ChildWidth
        {
            get => Convert.ToDouble(GetValue(ChildWidthProperty));
            set => SetValue(ChildWidthProperty, value);
        }
        public double ChildHeight
        {
            get => Convert.ToDouble(GetValue(ChildHeightProperty));
            set => SetValue(ChildHeightProperty, value);
        }

        public int Columns
        {
            get => Convert.ToInt32(GetValue(ColumnsProperty));
            set => SetValue(ColumnsProperty, value);
        }

        public int Rows
        {
            get => Convert.ToInt32(GetValue(RowsProperty));
            set => SetValue(RowsProperty, value);
        }
        
        #endregion

        private int GetItemCount(DependencyObject element)
        {
            var itemsControl = ItemsControl.GetItemsOwner(element);
            return itemsControl.HasItems ? itemsControl.Items.Count : 0;
        }

        private int CalculateChildrenCloumn(Size availableSize)
        {
            if (double.IsInfinity(availableSize.Width))
                return Columns;

            // 获取ListBoxItem的Margin（假设所有项使用相同的Margin）
            Thickness itemMargin = GetItemMargin();

            // 计算每个项占用的总宽度（内容宽度 + 左右Margin）
            double itemTotalWidth = 100 + itemMargin.Left + itemMargin.Right; // 100是最小内容宽度

            // 计算可容纳的项数（向下取整）
            int childrenPerRow = Math.Max(1, (int)(availableSize.Width / itemTotalWidth));

            // 不超过PerRowCount的限制
            return Math.Min(childrenPerRow, Columns);
        }

        private int CalculateChildrenRow(Size availableSize)
        {
            return Rows;         
        }

        /// <summary>
        /// width不超过availableSize的情况下，自身实际需要的Size(高度可能会超出availableSize)
        /// </summary>
        /// <param name="availableSize"></param>
        /// <param name="itemsCount"></param>
        /// <returns></returns>
        private Size CalculateExtent(Size availableSize, int itemsCount)
        {
            int childPerRow = CalculateChildrenCloumn(availableSize); //现有宽度下 一行可以最多容纳多少个
            return new Size(childPerRow * this.ChildWidth, this.ChildHeight * Math.Ceiling(Convert.ToDouble(itemsCount) / childPerRow));
        }

        /// <summary>
        /// 更新滚动条
        /// </summary>
        /// <param name="availableSize"></param>
        private void UpdateScrollInfo(Size availableSize)
        {
            var extent = CalculateExtent(availableSize, GetItemCount(this));//extent 自己实际需要
            if (extent != this._extent)
            {
                this._extent = extent;
                this.ScrollOwner.InvalidateScrollInfo();
            }
            if (availableSize != this._viewPort)
            {
                this._viewPort = availableSize;
                this.ScrollOwner.InvalidateScrollInfo();
            }
        }
        /// <summary>
        /// 获取所有item，在可视区域内第一个item和最后一个item的索引
        /// </summary>
        /// <param name="firstIndex"></param>
        /// <param name="lastIndex"></param>
        private void GetVisibleRange(ref int firstIndex, ref int lastIndex)
        {
            int childPerRow = CalculateChildrenCloumn(this._extent);
            firstIndex = Convert.ToInt32(Math.Floor(this._offset.Y / this.ChildHeight)) * childPerRow;
            lastIndex = Convert.ToInt32(Math.Ceiling((this._offset.Y + this._viewPort.Height) / this.ChildHeight)) * childPerRow - 1;
            int itemsCount = GetItemCount(this);
            if (lastIndex >= itemsCount)
                lastIndex = itemsCount - 1;
        }

        /// <summary>
        /// 将不在可视区域内的item 移除
        /// </summary>
        /// <param name="startIndex">可视区域开始索引</param>
        /// <param name="endIndex">可视区域结束索引</param>
        private void CleanUpItems(int startIndex, int endIndex)
        {
            var children = this.InternalChildren;
            var generator = this.ItemContainerGenerator;
            for (int i = children.Count - 1; i >= 0; i--)
            {
                try
                {
                    var childGeneratorPosition = new GeneratorPosition(i, 0);
                    // 验证位置有效性
                    if (childGeneratorPosition.Index < 0 || childGeneratorPosition.Index >= children.Count)
                        continue;

                    int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPosition);
                    // 验证索引范围
                    if (itemIndex < 0 || itemIndex >= GetItemCount(this))
                        continue;

                    var isGeneratorActive = ItemContainerGenerator?.GetItemContainerGeneratorForPanel(this) != null;
                    if (itemIndex < startIndex || itemIndex > endIndex)
                    {
                        if (isGeneratorActive)
                        {
                            try
                            {
                                generator.Remove(childGeneratorPosition, 1);
                                RemoveInternalChildRange(i, 1);
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                }
            }
        }

        /// <summary>
        /// scroll/availableSize/添加删除元素 改变都会触发  edit元素不会改变
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            // 计算每行子项数量和子项宽度
            int childrenCloumn = CalculateChildrenCloumn(availableSize);
            ChildWidth = availableSize.Width / childrenCloumn;
            int childrenRow =  CalculateChildrenRow(availableSize);
            ChildHeight = availableSize.Height / childrenRow;
            //availableSize更新后，更新滚动条
            UpdateScrollInfo(availableSize);
            //availableSize更新后，获取当前viewport内可放置的item的开始和结束索引  firstIndex-lastIndex之间的item可能部分在viewport中也可能都不在viewport中。
            var firstVisibleIndex = 0;
            var lastVisibleIndex = 0;
            GetVisibleRange(ref firstVisibleIndex, ref lastVisibleIndex);

            //因为配置了虚拟化，所以children的个数一直是viewport区域内的个数,如果没有虚拟化则是ItemSource的整个的个数
            UIElementCollection children = this.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            //获得第一个可被显示的item的位置
            GeneratorPosition startPosition = generator.GeneratorPositionFromIndex(firstVisibleIndex);
            int childIndex = (startPosition.Offset == 0) ? startPosition.Index : startPosition.Index + 1;//startPosition在children中的索引
            using (generator.StartAt(startPosition, GeneratorDirection.Forward, true))
            {
                int itemIndex = firstVisibleIndex;
                while (itemIndex <= lastVisibleIndex)//生成lastVisibleIndex-firstVisibleIndex个item
                {
                    var child = generator.GenerateNext(out var newlyRealized) as UIElement;
                    if (newlyRealized)
                    {
                        if (childIndex >= children.Count)
                            base.AddInternalChild(child);
                        else
                        {
                            base.InsertInternalChild(childIndex, child);
                        }
                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        //处理 正在显示的child被移除了这种情况
                        if (!child.Equals(children[childIndex]))
                        {
                            base.RemoveInternalChildRange(childIndex, 1);
                        }
                    }

                    // 获取ListBoxItem的Margin
                    var itemMargin = GetItemMargin();
                    // 计算可用宽度（减去左右Margin）

                    double availableWidth = ChildWidth - itemMargin.Left - itemMargin.Right;
                    //child.DesiredSize//child想要的size
                    child.Measure(new Size(availableWidth, ChildHeight));
                    itemIndex++;
                    childIndex++;
                }
            }

            CleanUpItems(firstVisibleIndex, lastVisibleIndex);
            return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
        }

        private Thickness GetItemMargin()
        {
            if (InternalChildren.Count > 0 && InternalChildren[0] is FrameworkElement item)
            {
                return item.Margin;
            }
            return new Thickness(0);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var generator = this.ItemContainerGenerator;
            UpdateScrollInfo(finalSize);
            int childPerRow = CalculateChildrenCloumn(finalSize);
            double availableItemWidth = finalSize.Width / childPerRow;
            for (int i = 0; i <= this.Children.Count - 1; i++)
            {
                var child = this.Children[i];
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                int row = itemIndex / childPerRow;//current row
                int column = itemIndex % childPerRow;
                double xCorrdForItem = 0;

                xCorrdForItem = column * availableItemWidth + (availableItemWidth - this.ChildWidth) / 2;

                Rect rec = new Rect(xCorrdForItem, row * this.ChildHeight, this.ChildWidth, this.ChildHeight);
                child.Arrange(rec);
            }
            return finalSize;
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            this.SetVerticalOffset(this.VerticalOffset);
        }
        protected override void OnClearChildren()
        {
            base.OnClearChildren();
            this.SetVerticalOffset(0);
        }
        protected override void BringIndexIntoView(int index)
        {
            if (index < 0 || index >= Children.Count)
                throw new ArgumentOutOfRangeException();
            int row = index / CalculateChildrenCloumn(RenderSize);
            SetVerticalOffset(row * this.ChildHeight);
        }
        #region IScrollInfo Interface
        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }

        private Size _extent = new Size(0, 0);
        public double ExtentWidth => this._extent.Width;

        public double ExtentHeight => this._extent.Height;

        private Size _viewPort = new Size(0, 0);
        public double ViewportWidth => this._viewPort.Width;

        public double ViewportHeight => this._viewPort.Height;

        private Point _offset;
        public double HorizontalOffset => this._offset.X;

        public double VerticalOffset => this._offset.Y;

        public ScrollViewer ScrollOwner { get; set; }

        public void LineDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this.ScrollOffset);
        }

        public void LineLeft()
        {
            //throw new NotImplementedException();
        }

        public void LineRight()
        {
            //throw new NotImplementedException();
        }

        public void LineUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this.ScrollOffset);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        public void MouseWheelDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this.ScrollOffset);
        }

        public void MouseWheelLeft()
        {
            //throw new NotImplementedException();
        }

        public void MouseWheelRight()
        {
           // throw new NotImplementedException();
        }

        public void MouseWheelUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this.ScrollOffset);
        }

        public void PageDown()
        {
            this.SetVerticalOffset(this.VerticalOffset + this._viewPort.Height);
        }

        public void PageLeft()
        {
            //throw new NotImplementedException();
        }

        public void PageRight()
        {
            //throw new NotImplementedException();
        }

        public void PageUp()
        {
            this.SetVerticalOffset(this.VerticalOffset - this._viewPort.Height);
        }

        public void SetHorizontalOffset(double offset)
        {
            //throw new NotImplementedException();
        }

        public void SetVerticalOffset(double offset)
        {
            //if (offset < 0 || this._viewPort.Height >= this._extent.Height)
            //    offset = 0;
            //else
            //    if (offset + this._viewPort.Height >= this._extent.Height)
            //    offset = this._extent.Height - this._viewPort.Height;

            // 边界检查（保持你原有的逻辑）
            if (offset < 0 || this._viewPort.Height >= this._extent.Height)
                offset = 0;
            else if (offset + this._viewPort.Height >= this._extent.Height)
                offset = this._extent.Height - this._viewPort.Height;

            // 判断滚动方向
            bool scrollDown = offset > this.VerticalOffset;
            bool scrollUp = offset < this.VerticalOffset;

            double newOffset = offset;

            if (scrollDown)
            {
                // 向下翻页：滚动到下一页
                newOffset = this.VerticalOffset + this._viewPort.Height;
            }
            else if (scrollUp)
            {
                // 向上翻页：滚动到上一页
                newOffset = this.VerticalOffset - this._viewPort.Height;
            }

            // 再次边界检查
            if (newOffset < 0)
                newOffset = 0;
            else if (newOffset + this._viewPort.Height > this._extent.Height)
                newOffset = Math.Max(0, this._extent.Height - this._viewPort.Height);

            this._offset.Y = newOffset;
            this.ScrollOwner?.InvalidateScrollInfo();
            this._trans.Y = -newOffset;
            this.InvalidateMeasure();
            //接下来会触发MeasureOverride()
        }
        #endregion
    }
}
