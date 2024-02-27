using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixl
{
    internal class PolylineFilter : IFilter
    {
        public string Name { get; set; }
        List<(int, int)> Points {  get; set; }
        public PolylineFilter(string name, (int, int)[] points) 
        {
            Name = name;
            Points = [.. points];
            Points.Sort((x, y) => x.Item1.CompareTo(y.Item1)); 
            // https://stackoverflow.com/questions/4668525/sort-listtupleint-int-in-place
        }

        public void Apply(byte[] rgbValues, int width, int height, int bytes)
        {
            for (int counter = 0; counter < rgbValues.Length; counter++)
            {
                if (Points.Exists(x => x.Item1 == rgbValues[counter]))
                {
                    rgbValues[counter] = (byte)Points.Find(x => x.Item1 == rgbValues[counter]).Item2;
                }
                else
                {
                    // Index of a point from Points immediately to the right of the given RGB value
                    int rightBound = FindIndex(rgbValues[counter], Points, 0, Points.Count - 1);

                    // Slope of a function intersecting the right bound and the left bound (which
                    // is one unit to the left from the right bound)
                    double segmentSlope = (Points[rightBound].Item2 - Points[rightBound - 1].Item2)
                        / (Points[rightBound].Item1 - Points[rightBound - 1].Item1);

                    // Intercept of the function calculated based on the slope and the right bound
                    double segmentIntercept = Points[rightBound].Item2 - segmentSlope * Points[rightBound].Item1;

                    // New value of the RGB value calculated as a value of the function determined
                    // above
                    double newVal = (segmentSlope * rgbValues[counter]) + segmentIntercept;
                    rgbValues[counter] = (byte)(newVal);
                }
            }

        }

        public static int FindIndex(double target, List<(int, int)> list, int start, int end)
        {
            // Using binary search to find two values which are bounds for the target value
            
            int mid = (int)Math.Floor((start + end) / 2.0);

            if (target < list[mid].Item1)
            {
                if (mid == 0)
                {
                    throw new IndexOutOfRangeException();
                }
                if (target > list[mid - 1].Item1)
                    return mid;
                else
                {
                    return FindIndex(target, list, start, mid - 1);
                }
            }
            if (target > list[mid].Item1)
            {
                if (mid == list.Count - 1)
                {
                    throw new IndexOutOfRangeException();
                }
                if (target < list[mid + 1].Item1)
                    return mid + 1;
                else
                {
                    return FindIndex(target, list, mid + 1, end);
                }
            }
            return -1;
        }
    }
}
