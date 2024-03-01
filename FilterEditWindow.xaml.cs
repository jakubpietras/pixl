using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
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
        private ObservableCollection<PolylineFilter> PolylineFilters { get; set; }
        private PolylineFilter? SelectedFilter { get; set; }
        private Polyline GraphPolyline { get; set; }
        private ItemsControl GraphPoints { get; set; }  
        public FilterEditWindow(ObservableCollection<PolylineFilter> polylineFilters)
        {
            InitializeComponent();
            PolylineFilters = polylineFilters; 
            cmbFilters.ItemsSource = PolylineFilters;
            GraphPolyline = new Polyline();
            GraphPoints = new ItemsControl(); 
            GraphPoints.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas)));
            Graph.Children.Add(GraphPoints);
            DataContext = this;
        }
        private void cmbFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Graph.Children.Remove(GraphPolyline);
            PolylineFilter selectedFilter = cmbFilters.SelectedItem as PolylineFilter;
            if (selectedFilter != null)
            {
                // Save the filter for possible editing
                SelectedFilter = selectedFilter;

                // Draw points on the canvas
                GraphPolyline = new Polyline
                {
                    Stroke = System.Windows.Media.Brushes.Black,
                    StrokeThickness = 2,
                    FillRule = FillRule.EvenOdd
                };
                PointCollection points = new PointCollection();
                foreach (var p in SelectedFilter.Points)
                {
                    points.Add(new System.Windows.Point(p.Item1, 255 - p.Item2));
                }
                GraphPolyline.Points = points;
                Graph.Children.Add(GraphPolyline);


                GraphPoints.Items.Clear();

                foreach (var p in SelectedFilter.Points)
                {
                    Ellipse ellipse = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = System.Windows.Media.Brushes.Red,
                        Stroke = System.Windows.Media.Brushes.Red

                    };
                    GraphPoints.Items.Add(ellipse);
                    Canvas.SetLeft(ellipse, p.Item1 - 2.5);
                    Canvas.SetTop(ellipse, 255 - p.Item2 - 2.5);
                }
            }
        }
    }

    public class CoordsToPointsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            PointCollection pointCollection = new PointCollection();
            if (value is PolylineFilter filter)
            {
                foreach (var p in filter.Points)
                {
                    pointCollection.Add(new System.Windows.Point(p.Item1, p.Item2));
                }
            }
            return pointCollection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           
            throw new ArgumentException("Value must be a PointCollection", nameof(value));
        }
    }
}
