using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Drawing;

namespace Pixl
{
    internal class FilterProcessor
    {
        // Invoker for the commands (Command pattern)
        IFilter? _filter;
        Bitmap _bmp;
        FilterProcessor()
        {
            _filter = null;
        }
        public IFilter Filter { set { _filter = value; } }
        public void applyFilter()
        {
            if (_filter != null)
            {
                // Processing logic
            }
            else
            {
                throw new Exception("Filter must be chosen first!");
            }
        }
        
    }
}
