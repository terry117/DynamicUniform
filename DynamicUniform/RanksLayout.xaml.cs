using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DynamicUniform
{
    /// <summary>
    /// 排列布局 的交互逻辑
    /// </summary>
    public partial class RanksLayout : UserControl
    {
        /// <summary>
        /// 布局变更事件
        /// </summary>
        public event EventHandler<(int rows, int columns)> LayoutChanged; 
        public ObservableCollection<GridCell> GridCells { get; set; }
        public RanksLayout()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += RanksLayout_Loaded;
            InitializeGrid();
            InitializeMouseTracking();
        }

        private void RanksLayout_Loaded(object sender, RoutedEventArgs e)
        {
            LayoutImage.Source = (DrawingImage)FindResource("DrawingImage.1*1DrawingImage");
        }

        /// <summary>
        /// 初始化网格源
        /// </summary>
        private void InitializeGrid()
        {
            GridCells = new ObservableCollection<GridCell>();
            for (int row = 1; row <= TotalRow; row++)
            {
                for (int col = 1; col <= TotalColumn; col++)
                {
                    GridCells.Add(new GridCell(row, col));
                }
            }
        }

        private void InitializeMouseTracking()
        {
            BorderContainer.MouseMove += OnMouseMove;
            BorderContainer.MouseLeave += OnMouseLeave;
            BorderContainer.MouseUp += OnMouseUp;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            ClearSelection();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(ItemsContainer);
            UpdateSelection(mousePosition);
        }

        /// <summary>
        /// 更新网格选择布局
        /// </summary>
        /// <param name="relativePoint"></param>
        private void UpdateSelection(Point relativePoint)
        {
            var cellWidth = ItemsContainer.ActualWidth / TotalColumn;
            var cellHeight = ItemsContainer.ActualHeight / TotalRow;
            if (cellWidth <= 0 || cellHeight <= 0) return;
            // 计算当前行列
            var col = (int)Math.Max(0, Math.Min(TotalColumn, Math.Ceiling(relativePoint.X / cellWidth)));
            var row = (int)Math.Max(0, Math.Min(TotalRow, Math.Ceiling(relativePoint.Y / cellHeight)));
            if (row >= 1 && row <= TotalRow && col >= 1 && col <= TotalColumn)
            {
                if (_selectedRow != row || _selectedColumn != col)
                    UpdateGridColors(row, col);
            }
            else
            {
                ClearSelection();
            }
        }

        /// <summary>
        /// 更新网格颜色
        /// </summary>
        private void UpdateGridColors(int selectedRow, int selectedColumn)
        {
            _selectedRow = selectedRow;
            _selectedColumn = selectedColumn;
            foreach (var cell in GridCells)
            {
                cell.UpdateBackgroundColor(selectedRow, selectedColumn);
            }
        }

        /// <summary>
        /// 清空网格悬着
        /// </summary>
        private void ClearSelection()
        {
            _selectedRow = -1;
            _selectedColumn = -1;
            foreach (var cell in GridCells)
            {
                cell.ResetBackground();
            }
        }

        /// <summary>
        /// 弹窗
        /// </summary>
        private void ReferenceTarget_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = true;
            PopupSelect.Focus();
            e.Handled = true;
        }
        #region 布局变更
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            LayoutChanged?.Invoke(this, (_selectedRow, _selectedColumn));
            PopupSelect.IsOpen = false;
        }

        private void Layout1x1_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = false;
            LayoutImage.Source = (DrawingImage)FindResource("DrawingImage.1*1DrawingImage");
            LayoutChanged?.Invoke(this, (1, 1));
        }
        private void Layout2x2_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = false;
            LayoutImage.Source = (DrawingImage)FindResource("DrawingImage.2*2DrawingImage");
            LayoutChanged?.Invoke(this, (2, 2));
        }

        private void Layout3x3_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = false;
            LayoutImage.Source = (DrawingImage)FindResource("DrawingImage.3*3DrawingImage");
            LayoutChanged?.Invoke(this, (3, 3));
        }

        private void Layout4x4_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = false;
            LayoutImage.Source = (DrawingImage)FindResource("DrawingImage.4*4DrawingImage");
            LayoutChanged?.Invoke(this, (4, 4));
        }
        #endregion

        //已选行
        private int _selectedRow = -1;
        //已选列
        private int _selectedColumn = -1;
        //总行数
        private const int TotalRow = 6;
        //总列数
        private const int TotalColumn = 6;

        private void ReferenceTarget_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            PopupSelect.IsOpen = true;
            PopupSelect.Focus();
            e.Handled = true;
        }
    }

    /// <summary>
    /// 网格单元格数据模型
    /// </summary>
    public class GridCell : INotifyPropertyChanged
    {
        /// <summary>
        /// 网格行
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// 网格列
        /// </summary>
        public int Column { get; set; }

        public GridCell(int row, int column)
        {
            Row = row;
            Column = column;
            _originalBackground = new SolidColorBrush(Color.FromRgb(0x40, 0x40, 0x40));
            BackgroundColor = _originalBackground;
            _isHighlighted = false;
        }

        private Brush _backgroundColor;
        /// <summary>
        /// 网格背景色
        /// </summary>
        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                _backgroundColor = value;
                OnPropertyChanged(nameof(BackgroundColor));
            }
        }

        /// <summary>
        /// 更新网格背景色
        /// </summary>
        /// <param name="selectedRow">选中的行</param>
        /// <param name="selectedColumn">选中的列</param>
        public void UpdateBackgroundColor(int selectedRow, int selectedColumn)
        {
            if (Row <= selectedRow && Column <= selectedColumn)
            {
                if (_isHighlighted) return;
                var highlighted = Color.FromRgb(0xB4, 0xB4, 0xB4);
                BackgroundColor = new SolidColorBrush(highlighted);
                _isHighlighted = true;
            }
            else
            {
                BackgroundColor = _originalBackground;
                _isHighlighted= false;
            }
        }

        /// <summary>
        /// 重置网格的背景色
        /// </summary>
        public void ResetBackground()
        {
            BackgroundColor = _originalBackground;
            _isHighlighted = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //是否已设置高亮
        private bool _isHighlighted;
        //原始背景色
        private readonly Brush _originalBackground;
    }
}
