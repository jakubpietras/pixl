using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pixl
{
    /// <summary>
    /// Interaction logic for DitheringWindow.xaml
    /// </summary>
    public partial class DitheringWindow : Window
    {
        public class DitheringFilter
        {
            public string Name { get; set; }
            public int Size { get; set; }
            public double[,] Coefficients {  get; set; } 

            public DitheringFilter(string name, int size, double[,] coefficients)
            {
                Name = name;
                Size = size;
                Coefficients = coefficients;
            }
        };
        WriteableBitmap Bitmap { get; set; }
        ObservableCollection<DitheringFilter> ditheringFilters { get; set; }
        public DitheringWindow(WriteableBitmap bitmap)
        {
            InitializeComponent();
            Bitmap = bitmap;
            ditheringFilters =
            [
                // Floyd-Steinberg
                new DitheringFilter("Floyd-Steinberg", 3, new double[,] { { 0, 0, 0 }, { 0, 0, 7 / 16 }, { 3 / 16, 5 / 16, 1 / 16 } }),
                new DitheringFilter("Burkes", 3, new double[,] { { 0, 0, 0 }, { 0, 0, 7 / 16 }, { 3 / 16, 5 / 16, 1 / 16 } })
            ];
            cmbFilters.ItemsSource = ditheringFilters;
            DataContext = this;

        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            TabItem ti = DitherModeTab.SelectedItem as TabItem;
            if (ti != null)
            {
                if (ti.Name == "Color")
                {
                    MessageBox.Show("Color dither!", "Hi");
                    ColorDither();
                }
                else if (ti.Name == "Grayscale")
                {
                    MessageBox.Show("Gray dither!", "Hi");
                    GrayscaleDither();
                }
            }
            DialogResult = true;
            Close();
        }

        private void GrayscaleDither()
        {

        }

        private void ColorDither()
        {

        }
    }
}
