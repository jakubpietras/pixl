using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace Pixl
{
    public partial class MainWindow : Window
    { 
        BitmapImage? ImageDefault { get; set; }
        BitmapImage? ImageFiltered { get; set; }
        Bitmap? BitmapDefault { get; set; }
        Bitmap BitmapFiltered { get; set; }
        Dictionary<string, IFilter> Filters;
        FilterProcessor? Processor { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Processor = new FilterProcessor(BitmapFiltered);
            Filters = new Dictionary<string, IFilter>();
            Filters["Gamma-0.25"] = new GammaFilter("Gamma", 0.25f);
            Filters["Gamma-2"] = new GammaFilter("Gamma", 2f);
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                LoadImage(imagePath);
            }
        }

        



        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG file (*.png)|*.png";
            if (saveFileDialog.ShowDialog() == true)
            {
                // https://stackoverflow.com/questions/35804375/how-do-i-save-a-bitmapimage-from-memory-into-a-file-in-wpf-c
                BitmapImage imageToSave = BitmapToBitmapImage(BitmapFiltered);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(imageToSave));

                using (var fileStream = new System.IO.FileStream(saveFileDialog.FileName, System.IO.FileMode.Create))
                {
                    encoder.Save(fileStream);
                }
            }
        }
        private void LoadImage(string imagePath)
        {
            // https://www.c-sharpcorner.com/UploadFile/mahesh/using-xaml-image-in-wpf/
            BitmapImage bitmapImg = new BitmapImage();
            bitmapImg.BeginInit();
            bitmapImg.UriSource = new Uri(imagePath);
            bitmapImg.EndInit();
            ImageDefault = bitmapImg;

            // Bitmap from the original image
            Bitmap bitmap = new Bitmap(imagePath);
            BitmapFiltered = BitmapDefault = bitmap;
            Processor.BitmapFiltered = bitmap;

            // At first set both fields to the same image
            OriginalImage.Source = FilteredImage.Source = bitmapImg;
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            // https://stackoverflow.com/questions/94456/load-a-wpf-bitmapimage-from-a-system-drawing-bitmap
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void UpdateFilteredImage()
        {
            // Some handler for applying filters will do
            // processor.Filter = ...
            // processor.Bitmap = ...
            // processor.applyFilter()
            // FilteredImage.Source = Convert(processor.Bitmap)

            FilteredImage.Source = BitmapToBitmapImage(BitmapFiltered);

        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            Processor.Filter = Filters["Gamma-2"];
            Processor.applyFilter();
            UpdateFilteredImage();
        }
    }
}