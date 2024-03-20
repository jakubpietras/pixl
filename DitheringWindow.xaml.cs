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
            public int Columns { get; set; }
            public int Rows { get; set; } 
            public double[,] Coefficients {  get; set; } 

            public DitheringFilter(string name, int rows, int columns, double[,] coefficients)
            {
                Name = name;
                Rows = rows; // Number of rows in the kernel
                Columns = columns; // Number of columns in the kernel
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
                new DitheringFilter("Floyd-Steinberg", 3, 3, new double[,] { { 0, 0, 0 }, { 0, 0, 7.0 / 16 }, { 3.0 / 16, 5.0 / 16, 1.0 / 16 } }),
                new DitheringFilter("Burkes", 3, 5, new double[,] { {0, 0, 0, 0, 0 }, { 0, 0, 0, 8.0 / 32, 4.0 / 32}, {2.0 / 32, 4.0 / 32, 8.0 / 32, 4.0 / 32, 2.0 / 32 } }),
                new DitheringFilter("Stucky", 5, 5, new double [,] {{0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, {0, 0, 0, 8.0 / 42, 4.0 / 42 },
                    {2.0 / 42, 4.0 / 42, 8.0 / 42, 4.0 / 42, 2.0 / 42 }, {1.0 / 42, 2.0 / 42, 4.0 / 42, 2.0 / 42, 1.0 / 42 } }),
                new DitheringFilter("Sierra", 5, 5, new double [,]{{0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, {0, 0, 0, 5.0 / 32, 3.0 / 32}, {2.0 / 32, 4.0 / 32, 5.0 / 32, 4.0 / 32, 2.0 / 32},
                {0, 2.0 / 32, 3.0 / 32, 2.0 / 32, 0} }),
                new DitheringFilter("Atkinson", 5, 5, new double[,]{{0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, {0, 0, 0, 1.0 / 8, 1.0 / 8}, {0, 1.0 / 8, 1.0 / 8, 1.0 / 8, 0 }, { 0, 0, 1.0 / 8, 0, 0 } })
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

                for (int row = 0; row < Bitmap.PixelHeight; row++)
                {
                    for (int column = 0; column < Bitmap.PixelWidth; column++)
                    {
                        int rowSpan = (int)Math.Floor(df.Rows / 2.0);
                        int colSpan = (int)Math.Floor(df.Columns / 2.0);

                        unsafe
                        {
                            byte* pPixel = (byte*)pBackBuffer + row * stride + column * 4; // Assuming BGRA
                            int gray = (int)(0.2126 * pPixel[2] + 0.7152 * pPixel[1] + 0.0722 * pPixel[0]);
                            int approx = grayValues.Aggregate((x, y) => Math.Abs(x - gray) < Math.Abs(y - gray) ? x : y);

                            // Drawing a pixel
                            byte* pResultPixel = (byte*)(pResultBackBuffer + row * stride + column * 4);
                            pResultPixel[0] = (byte)approx;
                            pResultPixel[1] = (byte)approx;
                            pResultPixel[2] = (byte)approx;

                            // Error diffusion
                            int error = gray - approx;
                            for (int dr = -rowSpan; dr <= rowSpan; ++dr)
                            {
                                for (int dc = -colSpan; dc <= colSpan; ++dc)
                                {
                                    if (column + dc >= 0 && column + dc < Bitmap.PixelWidth && row + dr >= 0 && row + dr < Bitmap.PixelHeight)
                                    {
                                        byte* pNeighborPixel = pPixel + dr * stride + dc * 4;
                                        byte color = clampColor(pNeighborPixel[0] + error * df.Coefficients[dr + rowSpan, dc + colSpan]);
                                        pNeighborPixel[0] = color;
                                        pNeighborPixel[1] = color;
                                        pNeighborPixel[2] = color;
                                    }
                                    
                                }
                            }
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
            WriteableBitmap result = new WriteableBitmap(Bitmap);

            List<int> redValues = GenerateValues((int)sliderRed.Value);
            List<int> greenValues = GenerateValues((int)sliderGreen.Value);
            List<int> blueValues = GenerateValues((int)sliderBlue.Value);

            Bitmap.Lock();
            result.Lock();
            try
            {
                IntPtr pBackBuffer = Bitmap.BackBuffer;
                IntPtr pResultBackBuffer = result.BackBuffer;
                int stride = Bitmap.BackBufferStride;

                int pixelCount = Bitmap.PixelWidth * Bitmap.PixelHeight;
                for (int row = 0; row < Bitmap.PixelHeight; row++)
                {
                    for (int column = 0; column < Bitmap.PixelWidth; column++)
                    {
                        int rowSpan = (int)Math.Floor(df.Rows / 2.0);
                        int colSpan = (int)Math.Floor(df.Columns / 2.0);

                        unsafe
                        {
                            byte* pPixel = (byte*)pBackBuffer + row * stride + column * 4; // Assuming BGRA
                            int red = pPixel[2], green = pPixel[1], blue = pPixel[0];
                            int redApprox = redValues.Aggregate((x, y) => Math.Abs(x - red) < Math.Abs(y - red) ? x : y);
                            int greenApprox = greenValues.Aggregate((x, y) => Math.Abs(x - green) < Math.Abs(y - green) ? x : y);
                            int blueApprox = blueValues.Aggregate((x, y) => Math.Abs(x - blue) < Math.Abs(y - blue) ? x : y);


                            // Drawing a pixel
                            byte* pResultPixel = (byte*)(pResultBackBuffer + row * stride + column * 4);
                            pResultPixel[0] = (byte)blueApprox;
                            pResultPixel[1] = (byte)greenApprox;
                            pResultPixel[2] = (byte)redApprox;

                            // Error diffusion
                            int errorRed = red - redApprox;
                            int errorGreen = green - greenApprox;
                            int errorBlue = blue - blueApprox;

                            for (int dr = -rowSpan; dr <= rowSpan; ++dr)
                            {
                                for (int dc = -colSpan; dc <= colSpan; ++dc)
                                {
                                    if (column + dc >= 0 && column + dc < Bitmap.PixelWidth && row + dr >= 0 && row + dr < Bitmap.PixelHeight)
                                    {
                                        byte* pNeighborPixel = pPixel + dr * stride + dc * 4;
                                        pNeighborPixel[0] = clampColor(pNeighborPixel[0] + errorBlue * df.Coefficients[dr + rowSpan, dc + colSpan]);
                                        pNeighborPixel[1] = clampColor(pNeighborPixel[1] + errorGreen * df.Coefficients[dr + rowSpan, dc + colSpan]);
                                        pNeighborPixel[2] = clampColor(pNeighborPixel[2] + errorRed * df.Coefficients[dr + rowSpan, dc + colSpan]);
                                    }

                                }
                            }
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

            values.Sort();
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
