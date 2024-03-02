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
        private List<Point> FilterPoints { get; set; }
        private Rectangle Background { get; set; }
        private Path XAxisPath { get; set; }
        private Path YAxisPath { get; set; }

        public FilterEditWindow(ObservableCollection<PolylineFilter> polylineFilters)
        {
            InitializeComponent();
            PolylineFilters = polylineFilters;
            cmbFilters.ItemsSource = PolylineFilters;
            InitializeBackground();
            DataContext = this;
        }
        private void cmbFilters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PolylineFilter selectedFilter = cmbFilters.SelectedItem as PolylineFilter;
            FilterPoints = new List<Point>(selectedFilter.Points);
            if (selectedFilter != null)
            {
                // Save the filter for possible editing
                SelectedFilter = selectedFilter;
                DrawGraph();
            }
        }
        private void InitializeBackground()
        {
            Background = new Rectangle();
            Background.Width = 255;
            Background.Height = 255;
            Background.Fill = System.Windows.Media.Brushes.DimGray;
            Canvas.SetLeft(Background, 0);
            Canvas.SetTop(Background, 0);
            
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
            XAxisPath = new Path();
            XAxisPath.StrokeThickness = 1;
            XAxisPath.Stroke = Brushes.DarkGray;
            XAxisPath.Data = xAxis;
            
            GeometryGroup yAxis = new GeometryGroup();
            for (int y = 0; y < 255; y+=cellSize)
            {
                yAxis.Children.Add(new LineGeometry(
                    new Point(0, y), new Point(255, y)));
            }
            yAxis.Children.Add(new LineGeometry(
                new Point(0, 255), new Point(255, 255)));
            YAxisPath = new Path();
            YAxisPath.StrokeThickness = 1;
            YAxisPath.Stroke = Brushes.DarkGray;
            YAxisPath.Data = yAxis;
        }
        private void DrawBackground()
        {
            Graph.Children.Clear();
            Graph.Children.Add(Background);
            Graph.Children.Add(XAxisPath);
            Graph.Children.Add(YAxisPath);
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
            foreach(var p in FilterPoints)
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
            foreach (var p in FilterPoints)
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
        private void DrawGraph()
        {
            Graph.Children.Clear();
            DrawBackground();
            DrawPolyline();
            DrawPoints(6);
        }
        private void Graph_OnMouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(Graph);
            PositionIndicator.Content = $"X: {Math.Floor(p.X)} Y: {255 - Math.Floor(p.Y)}";
        }
        private void Graph_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if for a given x position, some point already exists
            
            // Add a point to collection
            FilterPoints.Add(new Point(Math.Floor(e.GetPosition(Graph).X), Math.Floor(255 - e.GetPosition(Graph).Y)));
            FilterPoints.Sort((p1, p2) => p1.X.CompareTo(p2.X));
            
            // Redraw points
            DrawGraph();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            SelectedFilter.Points = FilterPoints;
        }
    }
}
