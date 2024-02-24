using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixl
{
    internal class PolylineFilter : IFilter
    {
        string _name;
        SortedList<int, int> _points;
        PolylineFilter(string name) 
        {
            _name = name;
            _points = new SortedList<int, int>();
        }
        public string Name { get { return _name; } }
        public void Apply(byte[] rgbValues, int width, int height)
        {
            throw new NotImplementedException();
        }
        public void AddPoint(int x, int y)
        {
            _points.Add(x, y);
        }
        public void RemovePoint(int x)
        {
            _points.Remove(x);
        }
    }
}
