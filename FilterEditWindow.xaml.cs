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
using Point = System.Windows.Point;

namespace Pixl
{
    public partial class FilterEditWindow : Window
    {
        private ObservableCollection<PolylineFilter> PolylineFilters { get; set; }
        private PolylineFilter? SelectedFilter { get; set; }
        public FilterEditWindow(ObservableCollection<PolylineFilter> polylineFilters)
        {
            InitializeComponent();
            PolylineFilters = polylineFilters;
            cmbFilters.ItemsSource = PolylineFilters;
            DataContext = this;
        }
        private void cmbFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Graph.Children.Clear();
            PolylineFilter selectedFilter = cmbFilters.SelectedItem as PolylineFilter;
            if (selectedFilter != null)
            {
                // Save the filter for possible editing
                SelectedFilter = selectedFilter; 
                DrawPolyline();
                DrawPoints(6);
            }
        }

        private void DrawPolyline()
        {
            Polyline graphPolyline = new Polyline
            {
                Stroke = System.Windows.Media.Brushes.Black,
                StrokeThickness = 2,
                FillRule = FillRule.EvenOdd
            };
            List<Point> points = new List<Point>();
            foreach(var p in SelectedFilter.Points)
            {
                points.Add(new Point(p.X, 255 - p.Y));
            }
            PointCollection pointsCollection = new PointCollection(points);
            graphPolyline.Points = pointsCollection;
            Graph.Children.Add(graphPolyline);
        }
        private void DrawPoints(int width)
        {
            double radius = width / 2.0;
            ItemsControl graphPoints = new ItemsControl();
            graphPoints.ItemsPanel = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(Canvas)));
            foreach (var p in SelectedFilter.Points)
            {
                Ellipse ellipse = new Ellipse
                {
                    Width = width,
                    Height = width,
                    Fill = System.Windows.Media.Brushes.Red,
                    Stroke = System.Windows.Media.Brushes.Red

                };
                graphPoints.Items.Add(ellipse);
                Canvas.SetLeft(ellipse, p.X - radius);
                Canvas.SetTop(ellipse, 255 - p.Y - radius);
            }
            Graph.Children.Add(graphPoints);
        }
        private void DrawAxes(int step)
        {
            // TODO
        }
    }
}
