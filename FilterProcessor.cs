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
        IFilter? _filter;
        Bitmap? _bmp;
        FilterProcessor()
        {
            _filter = null;
            _bmp = null;
        }
        public IFilter Filter { set { _filter = value; } }
        public void applyFilter()
        {
            if (_filter != null && _bmp != null)
            {
                // Processing logic
                
            }
            else
            {
                throw new Exception("Error processing the image");
            }
        }
        
    }
}
