using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorVisualization.WPF;

public partial class VectorVisualizationView : Window
{
    private Point _vector1 = new(1, 0);
    private Point _vector2 = new(0, 1);
    private bool _draggingVector1;
    private bool _draggingVector2;
    private readonly double _step = 0.1;

    public VectorVisualizationView()
    {
        InitializeComponent();
        Draw();
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
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

    private void Window_MouseMove(object sender, MouseEventArgs e)
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

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
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
            DrawDotProduct();
            DrawCoordinates();
            DrawAngle();
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

        Canvas.SetLeft(circle, 300);
        Canvas.SetTop(circle, 200);
        DrawingCanvas.Children.Add(circle);
    }

    private void DrawVector(Point vector, Color color)
    {
        Point start = new(400, 300);
        Point end = new(400 + vector.X * 100, 300 - vector.Y * 100);

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

    private void DrawDotProduct()
    {
        double dotProduct = _vector1.X * _vector2.X + _vector1.Y * _vector2.Y;

        TextBlock textBlock = new()
        {
            Text = $"Скалярное произведение: {dotProduct:F6}",
            Foreground = Brushes.Black,
            FontSize = 14,
        };

        Canvas.SetLeft(textBlock, 10);
        Canvas.SetTop(textBlock, 10);
        DrawingCanvas.Children.Add(textBlock);
    }

    private void DrawCoordinates()
    {
        TextBlock textBlock1 = new()
        {
            Text = $"Координаты вектора 1: ({_vector1.X:F6}, {_vector1.Y:F6})",
            Foreground = Brushes.Red,
            FontSize = 14,
        };

        Canvas.SetLeft(textBlock1, 10);
        Canvas.SetTop(textBlock1, 30);
        DrawingCanvas.Children.Add(textBlock1);

        TextBlock textBlock2 = new()
        {
            Text = $"Координаты вектора 2: ({_vector2.X:F6}, {_vector2.Y:F6})",
            Foreground = Brushes.Blue,
            FontSize = 14,
        };

        Canvas.SetLeft(textBlock2, 10);
        Canvas.SetTop(textBlock2, 50);
        DrawingCanvas.Children.Add(textBlock2);
    }

    private void DrawAngle()
    {
        double dotProduct = _vector1.X * _vector2.X + _vector1.Y * _vector2.Y;
        double angle = Math.Acos(dotProduct) * (180.0 / Math.PI);

        TextBlock textBlock = new()
        {
            Text = $"Угол между векторами: {angle:F2}°",
            Foreground = Brushes.Black,
            FontSize = 14,
        };

        Canvas.SetLeft(textBlock, 10);
        Canvas.SetTop(textBlock, 70);
        DrawingCanvas.Children.Add(textBlock);
    }

    private bool IsPointOnVector(Point point, Point vector)
    {
        double x = 400 + vector.X * 100;
        double y = 300 - vector.Y * 100;
        return Math.Abs(point.X - x) < 10 && Math.Abs(point.Y - y) < 10;
    }

    private Point GetSnappedVector(Point point)
    {
        double centerX = 400;
        double centerY = 300;
        double dx = point.X - centerX;
        double dy = centerY - point.Y;
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
