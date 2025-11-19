using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DynamicUniform
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeTestDataAsync();
        }

        private async Task InitializeTestDataAsync()
        {
            var imageItems = new List<SeriesItem>();
            for (int i = 0; i < 20; i++)
            {
                var imageItem = new SeriesItem
                {
                    IsActualImage = true,
                    ImageText = (imageItems.Count + 1).ToString(),
                    FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testImage.jpg"),
                    UniqueId = Guid.NewGuid().ToString(),
                    Images = new ObservableCollection<FramesImage>(),
                };
                var image = await CreateBitmapImageAsync(imageItem.FilePath);
                for (int j = 1; j <= 5; j++)
                {
                    imageItem.Images.Add(new FramesImage() { Index = j, ImageSource = image });
                }
                imageItems.Add(imageItem);
            }

            SeriesRanksLayout.SetDataSource(imageItems);
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

        private void UpdateUniformGridLayout(int rows, int cols)
        {
            SeriesRanksLayout.UpdateUniformGridLayout(rows, cols);
        }

        /// <summary>
        /// 序列布局改变
        /// </summary>
        private void SeriesRanksLayout_OnLayoutChanged(object sender, (int rows, int columns) e)
        {
            SeriesRanksLayout.UpdateUniformGridLayout(e.rows, e.columns);
        }

        /// <summary>
        /// 图像布局改变
        /// </summary>
        private void PictureRanksLayout_OnLayoutChanged(object sender, (int rows, int columns) e)
        {
            SeriesRanksLayout.OnPictureLayoutChanged(e.rows, e.columns);
        }
    }
}
