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
        string _name;
        SortedList<int, int> _points;
        PolylineFilter(string name, (int, int)[] points) 
        {
            _name = name;
            _points = new SortedList<int, int>();
            foreach (var point in points)
            {
                if (point.Item2 > 255)
                    _points.Add(point.Item1, 255);
                else 
                    _points.Add(point.Item1, point.Item2);
            }
        }
        public string Name { get { return _name; } }

        public void AddPoint(int x, int y)
        {
            _points.Add(x, y);
        }
        public void RemovePoint(int x)
        {
            _points.Remove(x);
        }

        public void Apply(Bitmap bmp)
        {
            throw new NotImplementedException();
        }
    }
}
