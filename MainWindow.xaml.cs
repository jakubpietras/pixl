using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using Point = System.Windows.Point;

namespace Pixl
{
    public partial class MainWindow : Window
    { 
        WriteableBitmap? BitmapDefault { get; set; }
        WriteableBitmap? BitmapFiltered { get; set; }
        Dictionary<string, IFilter> Filters;
        private ObservableCollection<PolylineFilter> PolylineFilters;
        public MainWindow()
        {
            InitializeComponent();

            // Polyline filters have editable points.
            PolylineFilters = new ObservableCollection<PolylineFilter>();
            // For other filters points are not defined.
            Filters = new Dictionary<string, IFilter>();

            // ----- Initial set of filters -----
            // Inversion
            List<Point> points = new List<Point>
            {
                new Point(0, 255),
                new Point(255, 0)
            };
            
            PolylineFilters.Add(new PolylineFilter("Inversion", points));

            // Brightness correction
            PolylineFilters.Add(new PolylineFilter("Brightness+10", BrightnessCorrectionPolyline(10)));
            PolylineFilters.Add(new PolylineFilter("Brightness-20", BrightnessCorrectionPolyline(-20)));

            // Contrast Enhancement
            PolylineFilters.Add(new PolylineFilter("Contrast", ContrastEnhancementPolyline(4)));

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
            
            // https://stackoverflow.com/questions/11212771/save-writeablebitmap-to-file-using-wpf
            if (saveFileDialog.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                {
                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(BitmapFiltered));
                    encoder.Save(fs);
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

            // Bitmap from the original image
            BitmapDefault = new WriteableBitmap(bitmapImg);
            BitmapFiltered = new WriteableBitmap(bitmapImg);
            OriginalImage.Source = BitmapDefault;
            FilteredImage.Source = BitmapFiltered;
        }
        private void UpdateFilteredImage()
        {
            FilteredImage.Source = BitmapFiltered;
        }
        private List<Point> BrightnessCorrectionPolyline(int a)
        {
            if (a > 0 && a <= 255)
            {
                return new List<Point>
                {
                    new Point(0, a),
                    new Point(255 - a, 255),
                    new Point(255, 255)
                };
            }
            if (a < 0 && a >= -255)
            {
                return new List<Point>
                {
                    new Point(0, 0),
                    new Point(- a, 0),
                    new Point(255, 255 + a)
                };
            }
            return new List<Point>
            {
                new Point(0, 0),
                new Point(255, 255)
            };
        }
        private List<Point> ContrastEnhancementPolyline(int a)
        {
            if (a > 0)
            {
                int clampLeft = (int)Math.Floor(0.5 * 255 - 0.5 * 255 / a);
                int clampRight = (int)Math.Floor(0.5 * 255 + 0.5 * 255 / a);

                return new List<Point>
                {
                    new Point(0, 0),
                    new Point(clampLeft, 0),
                    new Point(clampRight, 255),
                    new Point(255, 255)
                };
            }
            return new List<Point>
            {
                new Point(0, 0),
                new Point(255, 255)
            };
        }
        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (sender as Button).Content.ToString();
            if (filterName != null)
            {
                var filter = Filters[filterName];
                filter.Apply(BitmapFiltered);
                UpdateFilteredImage();
            }
        }
        private void PolylineFilter_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (sender as Button).Content.ToString();
            if (filterName != null)
            {
                var filter = PolylineFilters.FirstOrDefault(filter => filter.Name == filterName);
                filter.Apply(BitmapFiltered);
                UpdateFilteredImage();
            }
        }
        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            BitmapFiltered = new WriteableBitmap(BitmapDefault);
            UpdateFilteredImage();
        }
        private void EditFilters_Click(object sender, RoutedEventArgs e)
        {
            var filterEditWindow = new FilterEditWindow(PolylineFilters);
            filterEditWindow.Show();
        }
    }
}