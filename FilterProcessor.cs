using System.Drawing;

namespace Pixl
{
    internal static class FilterProcessor
    {
        public static void applyFilter(Bitmap bitmapFiltered, IFilter filter)
        {
            if (filter == null || bitmapFiltered == null)
            {
                throw new Exception("Filter must be chosen");
            }
            else
            {
                // Locking the bitmap bits
                Rectangle rect = new Rectangle(0, 0, bitmapFiltered.Width, bitmapFiltered.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bitmapFiltered.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // Getting the address of the first line
                IntPtr bmpPtr = bmpData.Scan0;

                // Byte arrays
                int bytes = Math.Abs(bmpData.Stride) * bitmapFiltered.Height;
                byte[] rgbValues = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bmpPtr, rgbValues, 0, bytes);


                filter.Apply(rgbValues, bitmapFiltered.Width, bitmapFiltered.Height, bytes);
                
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpPtr, bytes);

                // Unlocking the bits
                bitmapFiltered.UnlockBits(bmpData);
            }   
        }  
    }
}
