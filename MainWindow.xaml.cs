using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace graficzka7
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDrawingStarted = false;
        private bool isDrawingWithMouse = false;
        private bool isPolygonSelected = false;
        private bool isMovingByMouse = false;

        private Point figureStart;
        private Point figureCurrentPoint;
        private Point tmpPolygonPoint;
        private Point initialMousePoint;
        private Point currentMousePoint;

        private Polygon currentDrawnPolygon;
        private List<PointCollection> polygons = new List<PointCollection>();
        private Polygon selectedPolygon;

        private Point tmpVector;
        private PointCollection initialPointsValue;

        public MainWindow()
        {
            InitializeComponent();
            endDrawing_button.IsEnabled = false;
            main_canvas.MouseLeftButtonUp += Main_canvas_MouseLeftButtonUp;
            main_canvas.MouseRightButtonUp += Main_canvas_MouseRightButtonUp;
        }

        private void Main_canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Main_canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMovingByMouse)
            {
                isMovingByMouse = false;
            }

        }

        private void DrawPolygon(Point point)
        {
            if (!isDrawingStarted)
            {
                isDrawingStarted = true;
                figureStart = point;
                currentDrawnPolygon = new Polygon();
                currentDrawnPolygon.Stroke = Brushes.Black;
                currentDrawnPolygon.StrokeThickness = 7;

                currentDrawnPolygon.Points.Add(figureStart);

                currentDrawnPolygon.MouseLeftButtonDown += polygon_MouseLeftButtonDown;

                main_canvas.Children.Add(currentDrawnPolygon);
            }
            else
            {
                figureCurrentPoint = point;
                currentDrawnPolygon.Points.Add(figureCurrentPoint);
            }
        }

        private void StopDrawing()
        {
            isDrawingStarted = false;
            polygons.Add(currentDrawnPolygon.Points);
        }

        private void polygon_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            if (isDrawingStarted) return;
            var polygon = sender as Polygon;
            SelectShape(polygon);
            isMovingByMouse = true;
        }

        private void SelectShape(Polygon polygon)
        {
            if (selectedPolygon != null) UnselectPolygon();

            isPolygonSelected = true;
            polygon.Stroke = Brushes.Red;
            selectedPolygon = polygon;
            SetIntialValues();
        }

        private void SetIntialValues()
        {
            initialMousePoint = Mouse.GetPosition(main_canvas);
            initialPointsValue = selectedPolygon.Points;
        }

        private void UnselectPolygon()
        {
            if(selectedPolygon != null)
            {
                selectedPolygon.Stroke = Brushes.Black;
                isPolygonSelected = false;
                selectedPolygon = null;
            }
            
        }

        private void MoveShapeByMouse()
        {
            tmpVector.X = currentMousePoint.X - initialMousePoint.X;
            tmpVector.Y = currentMousePoint.Y - initialMousePoint.Y;

            MoveSelectedPolygonByVector(tmpVector);
        }

        private void main_canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isPolygonSelected)
            {
                SetIntialValues();
            }   
            else if(!isPolygonSelected)
            {
                DrawPolygon(Mouse.GetPosition(main_canvas));
                isDrawingWithMouse = true;
            }
        }

        private void main_canvas_MouseMove(object sender, MouseEventArgs e)
        {
            currentMousePoint = Mouse.GetPosition(main_canvas);
            debug.Content = $"X: {currentMousePoint.X} Y: {currentMousePoint.Y}";

            if (isPolygonSelected && Keyboard.IsKeyDown(Key.LeftCtrl) && e.LeftButton == MouseButtonState.Pressed) 
                RotateWithMouse();
            else if (isPolygonSelected && Keyboard.IsKeyDown(Key.LeftAlt) && e.LeftButton == MouseButtonState.Pressed)
                ScaleWithMouse();
            else if (isMovingByMouse && e.LeftButton == MouseButtonState.Pressed) MoveShapeByMouse();
            else if (isDrawingStarted && isDrawingWithMouse)
            {
                if(currentDrawnPolygon.Points.Contains(tmpPolygonPoint)) 
                    currentDrawnPolygon.Points.Remove(tmpPolygonPoint);
                tmpPolygonPoint = Mouse.GetPosition(main_canvas);
                currentDrawnPolygon.Points.Add(tmpPolygonPoint);
            }
        }

        private void ScaleWithMouse()
        {
            ScaleSelectedPolygon(initialMousePoint, new Point(currentMousePoint.X - initialMousePoint.X == 0 ? 1 : (currentMousePoint.X - initialMousePoint.X) * 0.1,
                currentMousePoint.Y - initialMousePoint.Y == 0 ? 1 : (currentMousePoint.Y - initialMousePoint.Y) * 0.1));
        }

        private void RotateWithMouse()
        {
            RotateSelectedPolygon(initialMousePoint, currentMousePoint.Y - initialMousePoint.Y);
        }

        private void main_canvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(isDrawingWithMouse)
            {
                StopDrawing();
                isDrawingWithMouse = false;
            }
            else if(isPolygonSelected)
            {
                UnselectPolygon();
            }
            
            
        }

        private void addPoint_button_Click(object sender, RoutedEventArgs e)
        {
            var point = ParseDrawingPoint();
            if (point != null) DrawPolygon((Point)point);
            endDrawing_button.IsEnabled = true;
        }

        private void endDrawing_button_Click(object sender, RoutedEventArgs e)
        {
            StopDrawing();
            endDrawing_button.IsEnabled = false;
        }

        private void save_button_Click(object sender, RoutedEventArgs e)
        {
            string x = JsonConvert.SerializeObject(polygons);
            File.WriteAllText("data.txt", x);
        }

        private void load_button_Click(object sender, RoutedEventArgs e)
        {
            ClearBoard();
            var data = File.ReadAllText("data.txt");
            polygons = JsonConvert.DeserializeObject<List<PointCollection>>(data);

            if(polygons != null)
            {
                foreach(var p in polygons)
                {
                    currentDrawnPolygon = new Polygon();
                    currentDrawnPolygon.Stroke = Brushes.Black;
                    currentDrawnPolygon.StrokeThickness = 7;

                    currentDrawnPolygon.Points = p;

                    currentDrawnPolygon.MouseLeftButtonDown += polygon_MouseLeftButtonDown;

                    main_canvas.Children.Add(currentDrawnPolygon);
                }
            }
        }

        private void ClearBoard()
        {
            UnselectPolygon();
            main_canvas.Children.Clear();
            polygons.Clear();
        }

        private Point? ParseDrawingPoint()
        {
            return ParsePointFromString(x_input.Text, y_input.Text);
        }

        private Point? ParseVectorPoint()
        {
            return ParsePointFromString(translation_x_input.Text, translation_y_input.Text);
        }

        private Point? ParseScaleVector()
        {
            return ParsePointFromString(scale_x_input.Text, scale_y_input.Text);
        }

        private Point? ParsePointFromString(string input1Text, string input2Text)
        {
            Point tmp = new Point();

            if (String.IsNullOrEmpty(input1Text)) input1Text = "0";
            if (String.IsNullOrEmpty(input2Text)) input2Text = "0";

            List<bool> canBeParsed = new List<bool>();
            double output;

            canBeParsed.Add(Double.TryParse(input1Text, out output));
            tmp.X = output;

            canBeParsed.Add(Double.TryParse(input2Text, out output));
            tmp.Y = output;

            if (canBeParsed.Any(p => p == false))
            {
                MessageBox.Show("Wpisz prawidłowe wartośći >:(");
                return null;
            }

            x_input.Text = "";
            y_input.Text = "";

            return tmp;
        }

        private double? ParseValue()
        {
            double value;
            if (Double.TryParse(value_input.Text, out value)) return value;
            MessageBox.Show("Wpisz prawidłowe wartośći >:(");
            return null;
        }
         
        private void MoveSelectedPolygonByVector(Point vector)
        {
            PointCollection tmp = new PointCollection();

            for (int i = 0; i < initialPointsValue.Count; i++)
            {
                tmp.Add(new Point(initialPointsValue[i].X + vector.X, initialPointsValue[i].Y + vector.Y));
            }

            selectedPolygon.Points = tmp;
        }

        private void move_button_Click(object sender, RoutedEventArgs e)
        {
            if(selectedPolygon == null)
            {
                MessageBox.Show("Najpierw zaznacz wielokąt >:(");
                return;
            }

            initialPointsValue = selectedPolygon.Points;
            var input = ParseVectorPoint();
            if (input == null) return;
            Point vector = (Point)input;

            MoveSelectedPolygonByVector(vector);
        }

        private void RotateSelectedPolygon(Point rotationPoint, double angle)
        {
            PointCollection tmp = new PointCollection();

            double angleRadians = angle * Math.PI / 180;

            for (int i = 0; i < initialPointsValue.Count; i++)
            {
                tmp.Add(new Point(rotationPoint.X + ((initialPointsValue[i].X - rotationPoint.X) * Math.Cos(angleRadians)) - ((initialPointsValue[i].Y - rotationPoint.Y) * Math.Sin(angleRadians)),
                    rotationPoint.Y + ((initialPointsValue[i].X - rotationPoint.X) * Math.Sin(angleRadians)) + ((initialPointsValue[i].Y - rotationPoint.Y) * Math.Cos(angleRadians))));
            }

            selectedPolygon.Points = tmp;
        }

        private void rotate_button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPolygon == null)
            {
                MessageBox.Show("Najpierw zaznacz wielokąt >:(");
                return;
            }

            initialPointsValue = selectedPolygon.Points;
            var input = ParseVectorPoint();
            var valueInput = ParseValue();
            if (input == null || value_input == null) return;

            Point rotationPoint = (Point)input;
            double angle = (double)valueInput;

            RotateSelectedPolygon(rotationPoint, angle);
        }

        private void scale_button_Click(object sender, RoutedEventArgs e)
        {
            if (selectedPolygon == null)
            {
                MessageBox.Show("Najpierw zaznacz wielokąt >:(");
                return;
            }

            initialPointsValue = selectedPolygon.Points;
            var input = ParseVectorPoint();
            var scaleInput = ParseScaleVector();
            if (input == null || value_input == null || scaleInput == null) return;

            Point scalePoint = (Point)input;
            Point scaleVector = (Point)scaleInput;

            ScaleSelectedPolygon(scalePoint, scaleVector);
        }

        private void ScaleSelectedPolygon(Point scalePoint, Point scaleVector)
        {
            PointCollection tmp = new PointCollection();

            for (int i = 0; i < initialPointsValue.Count; i++)
            {
                tmp.Add(new Point(scalePoint.X + ((initialPointsValue[i].X - scalePoint.X) * scaleVector.X),
                    scalePoint.Y + ((initialPointsValue[i].Y - scalePoint.Y) * scaleVector.Y)));
            }

            selectedPolygon.Points = tmp;
        }

        private void clear_button_Click(object sender, RoutedEventArgs e)
        {
            ClearBoard();
        }
    }
}
