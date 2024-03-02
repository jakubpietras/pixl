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
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;


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
                DrawBackground();
                DrawPolyline();
                DrawPoints(6);
            }
        }
        private void DrawBackground()
        {
            Rectangle bg = new Rectangle();
            bg.Width = 255;
            bg.Height = 255;
            bg.Fill = System.Windows.Media.Brushes.DimGray;
            Canvas.SetLeft(bg, 0);
            Canvas.SetTop(bg, 0);
            Graph.Children.Add(bg);

            // http://www.csharphelper.com/howtos/howto_wpf_graph_points.html
            int cellSize = 32; // Width and height of a single cell in the grid
            GeometryGroup xAxis = new GeometryGroup();
            for (int x = 0; x < 255; x+=cellSize)
            {
                xAxis.Children.Add(new LineGeometry(
                    new Point(x, 0), new Point(x, 255)));
            }
            xAxis.Children.Add(new LineGeometry(
                new Point(255, 0), new Point(255, 255)));
            Path xAxisPath = new Path();
            xAxisPath.StrokeThickness = 1;
            xAxisPath.Stroke = Brushes.LightGray;
            xAxisPath.Data = xAxis;
            Graph.Children.Add(xAxisPath);
            
            GeometryGroup yAxis = new GeometryGroup();
            for (int y = 0; y < 255; y+=cellSize)
            {
                yAxis.Children.Add(new LineGeometry(
                    new Point(0, y), new Point(255, y)));
            }
            xAxis.Children.Add(new LineGeometry(
                new Point(0, 255), new Point(255, 255)));
            Path yAxisPath = new Path();
            yAxisPath.StrokeThickness = 1;
            yAxisPath.Stroke = Brushes.LightGray;
            yAxisPath.Data = yAxis;
            Graph.Children.Add(yAxisPath);
        }
        private void DrawPolyline()
        {
            Polyline graphPolyline = new Polyline
            {
                Stroke = System.Windows.Media.Brushes.LightGray,
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
                    Fill = System.Windows.Media.Brushes.WhiteSmoke,
                    Stroke = System.Windows.Media.Brushes.WhiteSmoke

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

        private void Graph_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Graph);
            PositionIndicator.Content = $"X: {Math.Floor(p.X)} Y: {255 - Math.Floor(p.Y)}";
        }
    }
}
