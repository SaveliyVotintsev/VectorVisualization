using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorVisualization.WPF;

public partial class MainWindow
{
    private double _step = 0.02;
    private double _vectorLenght = 1;
    private readonly double _diameter = 300;

    private Vector _vector1 = new(1, 0);
    private Vector _vector2 = new(0, 1);

    private bool _draggingVector1;
    private bool _draggingVector2;

    private Point _center;
    private double Radius => _diameter / 2;

    public MainWindow()
    {
        InitializeComponent();

        _center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);

        DrawingCanvas.MinHeight = _diameter + 50;
        DrawingCanvas.MinWidth = _diameter + 50;

        VectorLengthTextBox.Text = _vectorLenght.ToString(CultureInfo.InvariantCulture);

        Draw();
    }

    private void OnSetLengthClicked(object sender, RoutedEventArgs e)
    {
        string lenghtText = VectorLengthTextBox.Text.Replace(',', '.');

        if (double.TryParse(lenghtText, NumberStyles.Any, CultureInfo.InvariantCulture, out double newLength) == false
            || newLength <= 0)
        {
            MessageBox.Show("Введите допустимое положительное число для длины вектора.");
            return;
        }

        _vectorLenght = newLength;
        Draw();
    }

    private void OnStepChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _step = e.NewValue;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _center = new Point(DrawingCanvas.ActualWidth / 2, DrawingCanvas.ActualHeight / 2);
        Draw();
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (IsPointOnVector(e.GetPosition(DrawingCanvas), _vector1))
        {
            _draggingVector1 = true;
            Mouse.Capture(this);
        }
        else if (IsPointOnVector(e.GetPosition(DrawingCanvas), _vector2))
        {
            _draggingVector2 = true;
            Mouse.Capture(this);
        }
    }

    private void OnMouseMoved(object sender, MouseEventArgs e)
    {
        if (_draggingVector1)
        {
            _vector1 = GetSnappedVector(e.GetPosition(DrawingCanvas));
            Draw();
        }
        else if (_draggingVector2)
        {
            _vector2 = GetSnappedVector(e.GetPosition(DrawingCanvas));
            Draw();
        }
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        _draggingVector1 = false;
        _draggingVector2 = false;
        Mouse.Capture(null);
    }

    #region Drawing

    private void Draw()
    {
        try
        {
            DrawingCanvas.Children.Clear();

            DrawAxes();
            DrawCircle();
            DrawVector(_vector1, Colors.Red);
            DrawVector(_vector2, Colors.Blue);
            DrawCircleCenter();

            DrawProjection(_vector1, Colors.Red);
            DrawProjection(_vector2, Colors.Blue);

            DrawAngleArc(_vector1, _vector2, Colors.Green);

            Vector vector1 = _vector1 * _vectorLenght;
            Vector vector2 = _vector1 * _vectorLenght;

            UpdateInformation(vector1, vector2);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void DrawCircle()
    {
        Ellipse circle = new()
        {
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            Width = _diameter,
            Height = _diameter,
        };

        Canvas.SetLeft(circle, _center.X - circle.Width / 2);
        Canvas.SetTop(circle, _center.Y - circle.Height / 2);
        DrawingCanvas.Children.Add(circle);
    }

    private void DrawCircleCenter()
    {
        Ellipse centerDot = new()
        {
            Fill = Brushes.Black,
            Width = 5,
            Height = 5,
        };

        Canvas.SetLeft(centerDot, _center.X - centerDot.Width / 2);
        Canvas.SetTop(centerDot, _center.Y - centerDot.Height / 2);
        DrawingCanvas.Children.Add(centerDot);
    }

    private void DrawVector(Vector vector, Color color)
    {
        Point start = _center;
        Point end = new(_center.X + vector.X * Radius, _center.Y - vector.Y * Radius);

        Line line = new()
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
        };

        DrawingCanvas.Children.Add(line);
        DrawArrowHead(end, (Point)vector, color);
    }

    private void DrawAngleArc(Vector vector1, Vector vector2, Color color)
    {
        double arcAngle = Vector.AngleBetween(vector1 with
        {
            Y = -vector1.Y,
        }, vector2 with
        {
            Y = -vector2.Y,
        });

        double radius = Radius / 3;

        Point startPoint = new(_center.X + vector1.X * radius, _center.Y - vector1.Y * radius);
        Point endPoint = new(_center.X + vector2.X * radius, _center.Y - vector2.Y * radius);

        PathFigure pathFigure = new()
        {
            StartPoint = startPoint,
        };

        ArcSegment arcSegment = new()
        {
            Point = endPoint,
            Size = new Size(radius, radius),
            SweepDirection = arcAngle > 0
                ? SweepDirection.Clockwise
                : SweepDirection.Counterclockwise,
            IsLargeArc = Math.Abs(arcAngle) > 180,
        };

        pathFigure.Segments.Add(arcSegment);

        PathGeometry pathGeometry = new();
        pathGeometry.Figures.Add(pathFigure);

        Path arcPath = new()
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 1,
            StrokeDashArray = [6, 6],
            Data = pathGeometry,
        };

        DrawingCanvas.Children.Add(arcPath);

        double angle = Vector.AngleBetween(vector1, vector2);
        Point midPoint = new((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);

        TextBlock label = new()
        {
            Text = $"{angle:F2}°",
            Foreground = new SolidColorBrush(color),
            Background = Brushes.White,
            ToolTip = $"{angle:F2}°",
        };

        Canvas.SetLeft(label, midPoint.X - 20);
        Canvas.SetTop(label, midPoint.Y - 10);

        DrawingCanvas.Children.Add(label);
    }

    private void DrawAxes()
    {
        Line lineX = new()
        {
            Stroke = new SolidColorBrush(Colors.LightGray),
            StrokeThickness = 1,
            X1 = 0,
            Y1 = _center.Y,
            X2 = DrawingCanvas.ActualWidth,
            Y2 = _center.Y,
        };

        Line lineY = new()
        {
            Stroke = new SolidColorBrush(Colors.LightGray),
            StrokeThickness = 1,
            X1 = _center.X,
            Y1 = 0,
            X2 = _center.X,
            Y2 = DrawingCanvas.ActualHeight,
        };

        DrawingCanvas.Children.Add(lineX);
        DrawingCanvas.Children.Add(lineY);

        DrawArrowHead(new Point(lineX.X2, lineX.Y2), new Point(1, 0), Colors.LightGray);
        DrawArrowHead(new Point(lineY.X1, lineY.Y1), new Point(0, 1), Colors.LightGray);

        DrawText("X", lineX.X2 - 20, lineX.Y2 + 2);
        DrawText("Y", lineY.X1 + 10, lineY.Y1 + 2);
        DrawText("0", _center.X, _center.Y);

        string label = _vectorLenght.ToString("F3");

        DrawText(label, _center.X + Radius, _center.Y, 10);
        DrawText(label, _center.X, _center.Y - Radius - 15, 10);
        DrawText("-" + label, _center.X - Radius - 32, _center.Y, 10);
        DrawText("-" + label, _center.X, _center.Y + Radius, 10);
        return;

        void DrawText(string text, double left, double top, double fontSize = 14)
        {
            TextBlock textBlock = new()
            {
                Text = text,
                Foreground = Brushes.Black,
                FontSize = fontSize,
            };

            Canvas.SetLeft(textBlock, left);
            Canvas.SetTop(textBlock, top);
            DrawingCanvas.Children.Add(textBlock);
        }
    }

    private void DrawProjection(Vector vector, Color color)
    {
        Point start = _center;
        Point end = new(start.X + vector.X * Radius, start.Y - vector.Y * Radius);

        DrawLine(end.X, end.Y, end.X, start.Y);
        DrawLabel(end.X, start.Y, vector.X * _vectorLenght);

        DrawLine(end.X, end.Y, start.X, end.Y);
        DrawLabel(start.X, end.Y, vector.Y * _vectorLenght);
        return;

        void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line projectionLine = new()
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,
                StrokeDashArray = [2, 2],
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
            };

            DrawingCanvas.Children.Add(projectionLine);
        }

        void DrawLabel(double x, double y, double value)
        {
            TextBlock label = new()
            {
                Text = $"{value:F3}",
                Foreground = new SolidColorBrush(color),
                Background = new SolidColorBrush(Colors.White),
                ToolTip = $"{value:F3}",
            };

            Canvas.SetLeft(label, x + 2);
            Canvas.SetTop(label, y + 2);
            DrawingCanvas.Children.Add(label);
        }
    }

    private void DrawArrowHead(Point center, Point vector, Color color)
    {
        double arrowLength = 10;
        double arrowAngle = Math.PI / 6;

        double centerAngle = Math.Atan2(vector.Y, vector.X);

        DrawArrowLine(centerAngle - arrowAngle);
        DrawArrowLine(centerAngle + arrowAngle);
        return;

        void DrawArrowLine(double angle)
        {
            Point end = new(center.X - arrowLength * Math.Cos(angle), center.Y + arrowLength * Math.Sin(angle));

            Line arrowLine = new()
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 2,
                X1 = center.X,
                Y1 = center.Y,
                X2 = end.X,
                Y2 = end.Y,
            };

            DrawingCanvas.Children.Add(arrowLine);
        }
    }

    #endregion

    private void UpdateInformation(Vector vector1, Vector vector2)
    {
        double angle = Vector.AngleBetween(vector1, vector2);
        double dotProduct = Vector.Multiply(vector1, vector2);

        DotProductTextBlock.Text = $"{dotProduct:F3}";

        Vector1CoordinatesTextBlock.Text = $"({vector1.X:F3}, {vector1.Y:F3})";
        Vector2CoordinatesTextBlock.Text = $"({vector2.X:F3}, {vector2.Y:F3})";

        AngleTextBlock.Text = $"{angle:F2}°";
    }

    private bool IsPointOnVector(Point point, Vector vector)
    {
        double x = _center.X + vector.X * Radius;
        double y = _center.Y - vector.Y * Radius;
        return Math.Abs(point.X - x) < 10 && Math.Abs(point.Y - y) < 10;
    }

    private Vector GetSnappedVector(Point point)
    {
        Vector vector = new(point.X - _center.X, _center.Y - point.Y);
        vector.Normalize();

        double angle = Math.Atan2(vector.Y, vector.X) * (180.0 / Math.PI);
        double snappedAngle = Math.Round(angle / _step) * _step * (Math.PI / 180.0);

        double snappedX = Math.Cos(snappedAngle);
        double snappedY = Math.Sin(snappedAngle);

        return new Vector(snappedX, snappedY);
    }
}
