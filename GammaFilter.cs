using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;

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

        public void Apply(Bitmap bmp)
        {
            // Locking the bitmap bits
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Getting the address of the first line
            IntPtr bmpPtr = bmpData.Scan0;

            // Byte arrays
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValuesBmp = new byte[bytes];
            byte[] rgbValuesTmp = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(bmpPtr, rgbValuesBmp, 0, bytes);
            
            // Actual processing. Gamma filter - raising all values to the power
            for (int counter = 0; counter < rgbValuesBmp.Length; counter++)
            {
                double val = (double)rgbValuesBmp[counter] / 255;
                double newVal = Math.Pow(val, Gamma);
                rgbValuesTmp[counter] = (byte)(newVal * 255);
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValuesTmp, 0, bmpPtr, bytes);

            // Unlocking the bits
            bmp.UnlockBits(bmpData);
        }
    }
}
