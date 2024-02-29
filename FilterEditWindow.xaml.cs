using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Pixl
{
    public partial class FilterEditWindow : Window
    {
        public Dictionary<string, PolylineFilter> PolylineFilters;
        public FilterEditWindow(Dictionary<string, PolylineFilter> polylineFilters)
        {
            InitializeComponent();
            PolylineFilters = polylineFilters;
        }
    }
}
