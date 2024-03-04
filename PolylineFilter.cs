using System.Windows;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Pixl
{
    public class PolylineFilter : IFilter
    {
        public string Name { get; set; }
        public List<Point> Points {  get; set; }
        public PolylineFilter(string name, List<Point> points) 
        {
            Name = name;
            Points = points;
            Points.Sort((p1, p2) => p1.X.CompareTo(p2.X)); 
        }

        public void Apply(WriteableBitmap bitmap)
        {
            bitmap.Lock();
            try
            {
                IntPtr pBackBuffer = bitmap.BackBuffer;
                int pixelCount = bitmap.PixelWidth * bitmap.PixelHeight;
                unsafe
                {
                    for (int counter = 0; counter < pixelCount; counter++)
                    {
                        byte* pPixel = (byte*)pBackBuffer + counter * 4; // Assuming BGRA
                        pPixel[0] = ProcessColor(pPixel[0]);
                        pPixel[1] = ProcessColor(pPixel[1]);
                        pPixel[2] = ProcessColor(pPixel[2]);
                    }
                }
                bitmap.AddDirtyRect(new Int32Rect(0,0, bitmap.PixelWidth, bitmap.PixelHeight));
            }
            finally
            {
                bitmap.Unlock();
            }
        }

        private byte ProcessColor(byte color)
        {
            // If there is a point with a color value directly, find it
            int index = -1;
            for (int i = 0; i < Points.Count; i++)
            {
                if (Points[i].X == color)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                return (byte)Points[index].Y;
            }

            // Index of a point from Points immediately to the right of the given RGB value
            int rightBound = FindIndex(color, Points, 0, Points.Count - 1);

            // Slope of a function intersecting the right bound and the left bound (which
            // is one unit to the left from the right bound)
            double segmentSlope = (Points[rightBound].Y - Points[rightBound - 1].Y)
                                  / (Points[rightBound].X - Points[rightBound - 1].X);

            // Intercept of the function calculated based on the slope and the right bound
            double segmentIntercept = Points[rightBound].Y - segmentSlope * Points[rightBound].X;

            // New value of the RGB value calculated as a value of the function determined
            // above
            double newVal = segmentSlope * color + segmentIntercept;
            return (byte) newVal;
        }

        public static int FindIndex(double target, List<Point> list, int start, int end)
        {
            // Using binary search to find two values which are bounds for the target value
            
            int mid = (int)Math.Floor((start + end) / 2.0);

            if (target < list[mid].X)
            {
                if (mid == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                if (target > list[mid - 1].X)
                    return mid;
                return FindIndex(target, list, start, mid - 1);
            }
            if (target > list[mid].X)
            {
                if (mid == list.Count - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                if (target < list[mid + 1].X)
                    return mid + 1;
                return FindIndex(target, list, mid + 1, end);
            }
            return -1;
        }
    }
}
