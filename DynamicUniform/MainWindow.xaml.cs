using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DynamicUniform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<ImageItem> imageItems;
        private ScrollViewer scrollViewer;
        private VirtualizingUniformGrid virtualizingUniformGrid;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            ImageListBox.SelectedIndex = 0;
            _ = InitializeTestDataAsync();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            scrollViewer = FindVisualChild<ScrollViewer>(ImageListBox);
            virtualizingUniformGrid = FindVisualChild<VirtualizingUniformGrid>(ImageListBox);
            LayoutComboBox_SelectionChanged(null, null);
        }

        private async Task InitializeTestDataAsync()
        {
            imageItems = new List<ImageItem>(); 
            for (int i = 1; i <= 31; i++)
            {
                var imageItem = new ImageItem();
       
                imageItem.IsImage = false;
                imageItem.ImageText = i.ToString();
                //imageItem.FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testImage.jpg");
                //imageItem.ImageSource = await CreateBitmapImageAsync(imageItem.FilePath);
                imageItems.Add(imageItem);
            }

            var imageItem1 = new ImageItem();
            imageItem1.ImageText = (imageItems.Count+1).ToString();
            imageItem1.IsImage = true;
            imageItem1.FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testImage.jpg");
            imageItem1.ImageSource = await CreateBitmapImageAsync(imageItem1.FilePath);
            imageItems.Add(imageItem1);
            ImageListBox.ItemsSource = imageItems;
        }

        /// <summary>
        /// 动态改变布局
        /// </summary>
        private void LayoutComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
       
            if (LayoutComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string tag)
            {
                var dimensions = tag.Split(',');
                if (dimensions.Length == 2 &&
                    int.TryParse(dimensions[0], out int rows) &&
                    int.TryParse(dimensions[1], out int cols))
                {
                    UpdateUniformGridLayout(rows, cols);
                }
            }
        }
        private void UpdateUniformGridLayout(int rows, int cols)
        {
            scrollViewer?.ScrollToHome();
            if (virtualizingUniformGrid != null)
            {
                virtualizingUniformGrid.Rows = rows;
                virtualizingUniformGrid.Columns = cols;
            }
        }

        // 辅助方法：在可视化树中查找子元素
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private async Task<BitmapImage> CreateBitmapImageAsync(string imageFile)
        {
            return await Task.Run(() =>
            {
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.UriSource = new Uri(imageFile);
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            });
        }

        private void AddTestDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ClearDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RanksLayout_OnLayoutChanged(object sender, (int rows, int columns) e)
        {
            UpdateUniformGridLayout(e.rows, e.columns);
        }
    }
}
