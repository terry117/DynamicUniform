using System.Windows.Media;

namespace DynamicUniform
{
    public class ImageItem
    {        
        public bool IsImage { get; set; }
        public string ImageText { get; set; }
        public string FilePath { get; set; }
        public ImageSource ImageSource { get; set; }
    }
}
