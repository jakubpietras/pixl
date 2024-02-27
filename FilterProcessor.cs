using System.Drawing;

namespace Pixl
{
    internal class FilterProcessor
    {
        public IFilter? Filter { get; set; }
        public Bitmap BitmapFiltered { get; set; }
        public FilterProcessor(Bitmap bitmapFiltered)
        {
            Filter = null;
            BitmapFiltered = bitmapFiltered;
        }
        public void applyFilter()
        {
            if (Filter == null || BitmapFiltered == null)
            {
                throw new Exception("Filter must be chosen");
            }
            else
            {
                // Locking the bitmap bits
                Rectangle rect = new Rectangle(0, 0, BitmapFiltered.Width, BitmapFiltered.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    BitmapFiltered.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    BitmapFiltered.PixelFormat);

                // Getting the address of the first line
                IntPtr bmpPtr = bmpData.Scan0;

                // Byte arrays
                int bytes = Math.Abs(bmpData.Stride) * BitmapFiltered.Height;
                byte[] rgbValues = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bmpPtr, rgbValues, 0, bytes);


                Filter.Apply(rgbValues, BitmapFiltered.Width, BitmapFiltered.Height, bytes);
                
                System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bmpPtr, bytes);

                // Unlocking the bits
                BitmapFiltered.UnlockBits(bmpData);
            }   
        }  
    }
}
