using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using static System.Net.WebRequestMethods;
using System.Windows.Shapes;


namespace Pixl
{
    public partial class MainWindow : Window
    { 
        BitmapImage? ImageDefault { get; set; }
        BitmapImage? ImageFiltered { get; set; }
        Bitmap? BitmapDefault { get; set; }
        Bitmap? BitmapFiltered { get; set; }
        Dictionary<string, IFilter> Filters;
        Dictionary<string, PolylineFilter> PolylineFilters;
        FilterProcessor? Processor { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Processor = new FilterProcessor();

            // Polyline filters have editable points.
            PolylineFilters = new Dictionary<string, PolylineFilter>();
            // For other filters points are not defined.
            Filters = new Dictionary<string, IFilter>();

            // ----- Initial set of filters -----
            // Inversion
            List<(int, int)> points = [(0, 255), (255, 0)];
            PolylineFilters["Inversion"] = new PolylineFilter("Inversion", points);

            // Brightness correction
            PolylineFilters["Brightness+10"] = new PolylineFilter("Brightness+10", BrightnessCorrectionPolyline(10));
            PolylineFilters["Brightness-20"] = new PolylineFilter("Brightness-20", BrightnessCorrectionPolyline(-20));

            // Contrast Enhancement
            PolylineFilters["Contrast"] = new PolylineFilter("Contrast", ContrastEnhancementPolyline(4));
            var fil = new PolylineFilter("Inversion", points);

            // Gamma correction
            Filters["Gamma-0.25"] = new GammaFilter("Gamma", 0.25f);
            Filters["Gamma-2"] = new GammaFilter("Gamma", 2f);

            PolylineFiltersPanel.ItemsSource = PolylineFilters;
            FiltersPanel.ItemsSource = Filters;
            
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
        private void About_Click(object sender, RoutedEventArgs e)
        {
            // TODO
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
            BitmapDefault = new Bitmap(imagePath);
            BitmapFiltered = new Bitmap(BitmapDefault);

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
            FilteredImage.Source = BitmapToBitmapImage(BitmapFiltered);
        }
        private List<(int, int)> BrightnessCorrectionPolyline(int a)
        {
            if (a > 0 && a <= 255)
                return new List<(int, int)>([(0, a), (255 - a, 255), (255, 255)]);
            if (a < 0 && a >= -255)
                return new List<(int, int)>([(0, 0), (-a, 0), (255, 255 + a)]);
            return new List<(int, int)>([(0, 0), (255, 255)]);
        }
        private List<(int, int)> ContrastEnhancementPolyline(int a)
        {
            if (a > 0)
            {
                int clampLeft = (int)Math.Floor(0.5 * 255 - 0.5 * 255 / a);
                int clampRight = (int)Math.Floor(0.5 * 255 + 0.5 * 255 / a);

                return new List<(int, int)>([(0, 0), (clampLeft, 0), (clampRight, 255), (255, 255)]);
            }
            return new List<(int, int)>([(0, 0), (255, 255)]);
        }
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (sender as Button).Content.ToString();
            if (filterName != null)
            {
                Processor.Filter = Filters[filterName];
                Processor.applyFilter(BitmapFiltered);
                UpdateFilteredImage();
            }
        }
        private void PolylineFilter_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (sender as Button).Content.ToString();
            if (filterName != null)
            {
                Processor.Filter = PolylineFilters[filterName];
                Processor.applyFilter(BitmapFiltered);
                UpdateFilteredImage();
            }
        }
        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            BitmapFiltered = new Bitmap(BitmapDefault);
            UpdateFilteredImage();
        }
        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // TODO
        }
    }
}