using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorVisualization.WPF;

public partial class MainWindow : Window
{
    private double _step = 0.01;
    private double _vectorLenght = 1;
    private Point _vector1 = new(1, 0);
    private Point _vector2 = new(0, 1);
    private bool _draggingVector1;
    private bool _draggingVector2;
    private double _centerX;
    private double _centerY;

    public MainWindow()
    {
        InitializeComponent();

        _centerX = DrawingCanvas.ActualWidth / 2;
        _centerY = DrawingCanvas.ActualHeight / 2;

        DrawingCanvas.MinHeight = 200 + 50;
        DrawingCanvas.MinWidth = 200 + 50;

        VectorLengthTextBox.Text = _vectorLenght.ToString(CultureInfo.InvariantCulture);

        Draw();
    }

    private void OnSetLengthClicked(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(VectorLengthTextBox.Text, out double newLength) && newLength > 0)
        {
            _vectorLenght = newLength;
            Draw();
        }
        else
        {
            MessageBox.Show("Введите допустимое положительное число для длины вектора.");
        }
    }

    private void OnStepChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _step = e.NewValue;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        _centerX = DrawingCanvas.ActualWidth / 2;
        _centerY = DrawingCanvas.ActualHeight / 2;
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

    private void Draw()
    {
        try
        {
            DrawingCanvas.Children.Clear();

            DrawCircle();
            DrawVector(_vector1, Colors.Red);
            DrawVector(_vector2, Colors.Blue);
            DrawCircleCenter();

            UpdateInformation();
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
            Width = 200,
            Height = 200,
        };

        Canvas.SetLeft(circle, _centerX - circle.Width / 2);
        Canvas.SetTop(circle, _centerY - circle.Height / 2);
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

        Canvas.SetLeft(centerDot, _centerX - centerDot.Width / 2);
        Canvas.SetTop(centerDot, _centerY - centerDot.Height / 2);
        DrawingCanvas.Children.Add(centerDot);
    }

    private void DrawVector(Point vector, Color color)
    {
        Point start = new(_centerX, _centerY);
        Point end = new(_centerX + vector.X * (200 / 2d), _centerY - vector.Y * (200 / 2d));

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
        DrawArrowHead(end, vector, color);
    }

    private void DrawArrowHead(Point end, Point vector, Color color)
    {
        double arrowLength = 10;
        double arrowAngle = Math.PI / 6;

        double angle = Math.Atan2(vector.Y, vector.X);

        Point arrowPoint1 = new(end.X - arrowLength * Math.Cos(angle - arrowAngle),
            end.Y + arrowLength * Math.Sin(angle - arrowAngle));

        Point arrowPoint2 = new(end.X - arrowLength * Math.Cos(angle + arrowAngle),
            end.Y + arrowLength * Math.Sin(angle + arrowAngle));

        Line arrowLine1 = new()
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            X1 = end.X,
            Y1 = end.Y,
            X2 = arrowPoint1.X,
            Y2 = arrowPoint1.Y,
        };

        Line arrowLine2 = new()
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 2,
            X1 = end.X,
            Y1 = end.Y,
            X2 = arrowPoint2.X,
            Y2 = arrowPoint2.Y,
        };

        DrawingCanvas.Children.Add(arrowLine1);
        DrawingCanvas.Children.Add(arrowLine2);
    }

    private void UpdateInformation()
    {
        double dotProduct = _vector1.X * _vector2.X * _vectorLenght + _vector1.Y * _vector2.Y * _vectorLenght;
        DotProductTextBlock.Text = $"{dotProduct:F6}";

        Vector1CoordinatesTextBlock.Text = $"({_vector1.X * _vectorLenght:F6}, {_vector1.Y * _vectorLenght:F6})";
        Vector2CoordinatesTextBlock.Text = $"({_vector2.X * _vectorLenght:F6}, {_vector2.Y * _vectorLenght:F6})";

        double angle = Math.Acos(_vector1.X * _vector2.X + _vector1.Y * _vector2.Y) * (180.0 / Math.PI);
        AngleTextBlock.Text = $"{angle:F2}°";
    }

    private bool IsPointOnVector(Point point, Point vector)
    {
        double x = _centerX + vector.X * 100;
        double y = _centerY - vector.Y * 100;
        return Math.Abs(point.X - x) < 10 && Math.Abs(point.Y - y) < 10;
    }

    private Point GetSnappedVector(Point point)
    {
        double dx = point.X - _centerX;
        double dy = _centerY - point.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);

        double unitX = dx / length;
        double unitY = dy / length;

        double snappedX = Math.Round(unitX / _step) * _step;
        double snappedY = Math.Round(unitY / _step) * _step;

        double snappedLength = Math.Sqrt(snappedX * snappedX + snappedY * snappedY);

        double maxLength = 1.0;

        if (snappedLength > maxLength)
        {
            snappedX /= snappedLength;
            snappedY /= snappedLength;
        }
        else if (snappedLength < maxLength)
        {
            snappedX *= maxLength / snappedLength;
            snappedY *= maxLength / snappedLength;
        }

        return new Point(snappedX, snappedY);
    }
}
