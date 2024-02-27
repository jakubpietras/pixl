using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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
            if (Filter == null)
            {
                throw new Exception("Filter must be chosen");
            }
            else
            {
                Filter.Apply(BitmapFiltered);
            }
            
        }
        
    }
}
