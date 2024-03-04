using System.Windows;
using System.Windows.Media.Imaging;

namespace Pixl;

public class ConvolutionFilter : IFilter
{
    public string Name { get; }
    private int Size { get; set; }
    private int Divisor { get; set; }
    private int[] Coefficients { get; set; }

    public ConvolutionFilter(string name, int size, int[] coefficients, int divisor = -1)
    {
        if (coefficients.Count() != size * size)
        {
            throw new Exception("Bitch wtf u doing");
        }

        Name = name;
        Size = size;
        Coefficients = coefficients;
        if (divisor < 0)
        {
            Divisor = coefficients.Sum();
        }
        else
        {
            Divisor = divisor;
        }
    }
    public void Apply(WriteableBitmap bitmap)
    {
        WriteableBitmap result = new WriteableBitmap(bitmap);
        
        result.Lock();
        bitmap.Lock();
        try
        {
            IntPtr pBackBuffer = bitmap.BackBuffer;
            IntPtr pResultBackBuffer = result.BackBuffer;
            int stride = bitmap.BackBufferStride;
            
            for (int column = 0; column < bitmap.PixelHeight; column++)
            {
                for (int row = 0; row < bitmap.PixelWidth; row++)
                {
                    // Assuming the size is an odd number only
                    int span = (int)Math.Floor(Size / 2.0);
                    double sumRed = 0, sumGreen = 0, sumBlue = 0;

                    unsafe
                    {
                        // y - row of the cell in the neighborhood
                        for (int y = -span; y <= span; y++)
                        {
                            // x - column of the cell in the neighborhood
                            for (int x = -span; x <= span; x++)
                            {
                                // Wrapping the image
                                int neighborX = row + x,
                                    neighborY = column + y;
                                if (neighborX < 0) neighborX = 0;
                                if (neighborY < 0) neighborY = 0;
                                if (neighborX >= bitmap.PixelWidth) neighborX = bitmap.PixelWidth - 1;
                                if (neighborY >= bitmap.PixelHeight) neighborY = bitmap.PixelHeight - 1;

                                // Index of the neighboring pixel in the bitmap
                                int neighborIndex = neighborY * stride + neighborX * 4;
                                byte* pNeighbor = (byte*)pBackBuffer + neighborIndex;
                                sumRed += pNeighbor[2] * Coefficients[(y + span) * Size + (x + span)];
                                sumGreen += pNeighbor[1] * Coefficients[(y + span) * Size + (x + span)];
                                sumBlue += pNeighbor[0] * Coefficients[(y + span) * Size + (x + span)];
                            }
                        }
                        byte* pResultPixel = (byte*)(pResultBackBuffer + column * stride + row * 4);
                        pResultPixel[2] = clampColor(sumRed / Divisor);
                        pResultPixel[1] = clampColor(sumGreen / Divisor);
                        pResultPixel[0] = clampColor(sumBlue / Divisor);
                    }
                }
            }
            bitmap.WritePixels(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight), result.BackBuffer, stride * bitmap.PixelHeight, stride);
        }
        finally
        {
            bitmap.Unlock();
            result.Unlock(); 
        }

        
    }

    private byte clampColor(double value)
    {
        int result = (int)Math.Floor(value);
        if (result > 255) result = 255;
        if (result < 0) result = 0;
        return (byte)result;
    }
}