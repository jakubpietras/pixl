using System.Windows;
using System.Windows.Media.Imaging;

namespace Pixl
{
    internal class GammaFilter : IFilter
    {
        public string Name {  get; set; }
        public float Gamma { get; set; }
        public GammaFilter(string name, float gamma)
        {
            Name = name;
            Gamma = gamma;
        }
        public void Apply(WriteableBitmap bitmap)
        {
            IntPtr pBackBuffer = bitmap.BackBuffer;
            int pixelCount = bitmap.PixelHeight * bitmap.PixelWidth;

            bitmap.Lock();
            try
            {
                unsafe
                {
                    for (int counter = 0; counter < pixelCount; counter++)
                    {
                        IntPtr pPixel = pBackBuffer + counter * 4;
                        var pColor = (byte*)pPixel;
                        pColor[0] = ProcessColor(pColor[0]);
                        pColor[1] = ProcessColor(pColor[1]);
                        pColor[2] = ProcessColor(pColor[2]);
                    }
                }
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        private byte ProcessColor(byte color)
        {
            double val = (double)color / 255;
            double newVal = Math.Pow(val, Gamma);
            return (byte)(newVal * 255);
        }
    }
}
