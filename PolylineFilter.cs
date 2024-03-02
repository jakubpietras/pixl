using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
            // https://stackoverflow.com/questions/4668525/sort-listtupleint-int-in-place
        }

        public void Apply(byte[] rgbValues, int width, int height, int bytes)
        {
            for (int counter = 0; counter < rgbValues.Length; counter++)
            {
                // If there is a point with a color value directly, find it
                int index = -1;
                for (int i = 0; i < Points.Count; i++)
                {
                    if (Points[i].X == rgbValues[counter])
                    {
                        index = i;
                        break;
                    }
                }
                if (index != -1)
                {
                    rgbValues[counter] = (byte)Points[index].Y;
                }
                else
                {
                    // Index of a point from Points immediately to the right of the given RGB value
                    int rightBound = FindIndex(rgbValues[counter], Points, 0, Points.Count - 1);

                    // Slope of a function intersecting the right bound and the left bound (which
                    // is one unit to the left from the right bound)
                    double segmentSlope = (Points[rightBound].Y - Points[rightBound - 1].Y)
                        / (Points[rightBound].X - Points[rightBound - 1].X);

                    // Intercept of the function calculated based on the slope and the right bound
                    double segmentIntercept = Points[rightBound].Y - segmentSlope * Points[rightBound].X;

                    // New value of the RGB value calculated as a value of the function determined
                    // above
                    double newVal = (segmentSlope * rgbValues[counter]) + segmentIntercept;
                    rgbValues[counter] = (byte)(newVal);
                }
            }

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
                else
                {
                    return FindIndex(target, list, start, mid - 1);
                }
            }
            if (target > list[mid].X)
            {
                if (mid == list.Count - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                if (target < list[mid + 1].X)
                    return mid + 1;
                else
                {
                    return FindIndex(target, list, mid + 1, end);
                }
            }
            return -1;
        }

        private bool MatchPoint(Point p, int val)
        {
            return p.X == val;
        }
    }
}
