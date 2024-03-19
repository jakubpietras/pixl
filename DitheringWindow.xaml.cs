using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            DitheringFilter df = cmbFilters.SelectedItem as DitheringFilter;
            if (ti != null && df != null)
            {
                if (ti.Name == "Color")
                {
                    ColorDither(df);
                }
                else if (ti.Name == "Grayscale")
                {
                    GrayscaleDither(df);
                }
            }
            DialogResult = true;
            Close();
        }

        private void GrayscaleDither(DitheringFilter df)
        {
            WriteableBitmap result = new WriteableBitmap(Bitmap);

            List<int> grayValues = GenerateValues((int)sliderGray.Value);
            Bitmap.Lock();
            result.Lock();
            try
            {
                IntPtr pBackBuffer = Bitmap.BackBuffer;
                IntPtr pResultBackBuffer = result.BackBuffer;
                int stride = Bitmap.BackBufferStride;

                int pixelCount = Bitmap.PixelWidth * Bitmap.PixelHeight;

                for (int column = 0; column < Bitmap.PixelHeight; column++)
                {
                    for (int row = 0; row < Bitmap.PixelWidth; row++)
                    {
                        int span = (int)Math.Floor(df.Size / 2.0);

                        unsafe
                        {
                            byte* pPixel = (byte*)pBackBuffer + column * stride + row * 4; // Assuming BGRA
                            int gray = (int)(0.2126 * pPixel[2] + 0.7152 * pPixel[1] + 0.0722 * pPixel[0]);
                            int approx = grayValues.Aggregate((x, y) => Math.Abs(x - gray) < Math.Abs(y - gray) ? x : y);

                            // Drawing a pixel
                            byte* pResultPixel = (byte*)(pResultBackBuffer + column * stride + row * 4);
                            pResultPixel[0] = (byte)approx;
                            pResultPixel[1] = (byte)approx;
                            pResultPixel[2] = (byte)approx;
                        }
                    }
                }
                Bitmap.WritePixels(new Int32Rect(0, 0, Bitmap.PixelWidth, Bitmap.PixelHeight), result.BackBuffer, stride * Bitmap.PixelHeight, stride);
            }
            finally
            {
                Bitmap.Unlock();
                result.Unlock();
            }
        }
        private void ColorDither(DitheringFilter df)
        {

        }

        private List<int> GenerateValues(int levels)
        {
            var values = new List<int>();
            int step = (int)Math.Floor(256f / (levels - 1));
            values.Add(0);
            values.Add(255);
            for (int i = 1; i < levels - 1; i++)
            {
                values.Add(i * step);
            }
            return values;
        }
        private byte clampColor(double value)
        {
            int result = (int)Math.Floor(value);
            if (result > 255) result = 255;
            if (result < 0) result = 0;
            return (byte)result;
        }
    }
}
