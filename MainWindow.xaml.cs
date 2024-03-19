﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using Point = System.Windows.Point;
using System.IO.MemoryMappedFiles;

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
            PolylineFilters.Add(new PolylineFilter("Brightness +50", BrightnessCorrectionPolyline(50)));
            PolylineFilters.Add(new PolylineFilter("Brightness -50", BrightnessCorrectionPolyline(-50)));

            // Contrast Enhancement
            PolylineFilters.Add(new PolylineFilter("Contrast", ContrastEnhancementPolyline(4)));

            // Gamma correction
            Filters["Gamma-0.25"] = new GammaFilter("Gamma-0.25", 0.25f);
            Filters["Gamma-2"] = new GammaFilter("Gamma-2", 2f);
            
            // Blur
            Filters["Blur 4x4"] = new ConvolutionFilter("Blur 4x4", 3, [1, 1, 1, 1, 1, 1, 1, 1, 1]);
            Filters["Blur 5x5"] = new ConvolutionFilter("Blur 5x5", 5, Enumerable.Repeat(1, 5 * 5).ToArray());

            // Gaussian blur
            Filters["Gaussian sm."] = new ConvolutionFilter("Gaussian sm.", 3, [0, 1, 0, 1, 4, 1, 0, 1, 0]);

            // Sharpen
            int a = 1, b = 5, s = b - 4 * a;
            Filters["Sharpen"] = new ConvolutionFilter("Sharpen", 3, [0, -a / s, 0, -a / s, b / s, -a / s, 0, -a / s, 0]);

            // Diagonal edge detection
            Filters["Edge"] = new ConvolutionFilter("Edge", 3, [0, -1, 0, -1, 4, -1, 0, -1, 0], 1);

            // Emboss
            Filters["E-Emboss"] = new ConvolutionFilter("Emboss", 3, [-1, 0, 1, -1, 1, 1, -1, 0, 1]);
            Filters["S-Emboss"] = new ConvolutionFilter("Emboss2", 3, [-1, -1, -1, 0, 1, 0, 1, 1, 1]);
            Filters["SE-Emboss"] = new ConvolutionFilter("Emboss2", 3, [-1, -1, 0, -1, 1, 1, 0, 1, 1]);


            PolylineFiltersPanel.ItemsSource = PolylineFilters;
            FiltersPanel.ItemsSource = Filters;
        }
        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.tiff)|*.jpg;*.jpeg;*.png;*.bmp;*.tiff";
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
                if (BitmapFiltered != null)
                {
                    filter.Apply(BitmapFiltered);
                    UpdateFilteredImage();
                }
            }
        }
        private void PolylineFilter_Click(object sender, RoutedEventArgs e)
        {
            string filterName = (sender as Button).Content.ToString();
            if (filterName != null)
            {
                var filter = PolylineFilters.FirstOrDefault(filter => filter.Name == filterName);
                if (filter != null && BitmapFiltered != null)
                {
                    filter.Apply(BitmapFiltered);
                    UpdateFilteredImage();
                }
            }
        }
        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            if (BitmapDefault != null)
            {
                BitmapFiltered = new WriteableBitmap(BitmapDefault);
                UpdateFilteredImage();
            }
        }
        private void EditFilters_Click(object sender, RoutedEventArgs e)
        {
            var filterEditWindow = new FilterEditWindow(PolylineFilters);
            filterEditWindow.Owner = this;
            filterEditWindow.Show();
        }

        private void Dithering_Click(object sender, RoutedEventArgs e)
        {
            var ditherWindow = new DitheringWindow(BitmapFiltered);
            ditherWindow.Owner = this;
            if (ditherWindow.ShowDialog().Value)
            {
                MessageBox.Show("You did it girl!");
                UpdateFilteredImage();
            }
        }
    }
}