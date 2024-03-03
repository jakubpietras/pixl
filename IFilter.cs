using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Printing.IndexedProperties;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Pixl
{
    internal interface IFilter
    {
        string Name { get; }
        void Apply(WriteableBitmap bitmap);
    }
}
