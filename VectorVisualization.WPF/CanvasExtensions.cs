using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace VectorVisualization.WPF;

public static class CanvasExtensions
{
    public static void DrawCircle(this Canvas canvas, double diameter, Point center)
    {
        Ellipse circle = new()
        {
            Stroke = Brushes.Black,
            StrokeThickness = 3,
            Width = diameter,
            Height = diameter,
        };

        Canvas.SetLeft(circle, center.X - circle.Width / 2);
        Canvas.SetTop(circle, center.Y - circle.Height / 2);
        canvas.Children.Add(circle);
    }

    public static void DrawCircleCenter(this Canvas canvas, Point center)
    {
        Ellipse centerDot = new()
        {
            Fill = Brushes.Black,
            Width = 5,
            Height = 5,
        };

        Canvas.SetLeft(centerDot, center.X - centerDot.Width / 2);
        Canvas.SetTop(centerDot, center.Y - centerDot.Height / 2);
        canvas.Children.Add(centerDot);
    }

    public static void DrawVector(this Canvas canvas, Vector vector, Color color, Point center)
    {
        Point start = center;
        Point end = new(center.X + vector.X, center.Y - vector.Y);

        Line line = new()
        {
            Stroke = new SolidColorBrush(color),
            StrokeThickness = 4,
            X1 = start.X,
            Y1 = start.Y,
            X2 = end.X,
            Y2 = end.Y,
        };

        canvas.Children.Add(line);
        canvas.DrawArrowHead(end, (Point)vector, color);
    }

    public static void DrawAngleArc(this Canvas canvas, Vector vector1, Vector vector2, Color color, Point center, double diameter)
    {
        double arcAngle = Vector.AngleBetween(vector1 with
        {
            Y = -vector1.Y,
        }, vector2 with
        {
            Y = -vector2.Y,
        });

        double radius = diameter / 2 / 3;

        Point startPoint = new(center.X + vector1.X * radius, center.Y - vector1.Y * radius);
        Point endPoint = new(center.X + vector2.X * radius, center.Y - vector2.Y * radius);

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
            StrokeThickness = 3,
            StrokeDashArray = [3, 3],
            Data = pathGeometry,
        };

        canvas.Children.Add(arcPath);

        double angle = Vector.AngleBetween(vector1, vector2);
        Point midPoint = new((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);

        TextBlock label = new()
        {
            Text = $"{angle:F2}°",
            FontSize = 16,
            Foreground = new SolidColorBrush(color),
            Background = Brushes.White,
            ToolTip = $"{angle:F2}°",
        };

        Canvas.SetLeft(label, midPoint.X - 20);
        Canvas.SetTop(label, midPoint.Y - 10);

        canvas.Children.Add(label);
    }

    public static void DrawAxes(this Canvas canvas, Point center)
    {
        Line lineX = new()
        {
            Stroke = new SolidColorBrush(Colors.LightGray),
            StrokeThickness = 3,
            X1 = 0,
            Y1 = center.Y,
            X2 = canvas.ActualWidth,
            Y2 = center.Y,
        };

        Line lineY = new()
        {
            Stroke = new SolidColorBrush(Colors.LightGray),
            StrokeThickness = 3,
            X1 = center.X,
            Y1 = 0,
            X2 = center.X,
            Y2 = canvas.ActualHeight,
        };

        canvas.Children.Add(lineX);
        canvas.Children.Add(lineY);

        canvas.DrawArrowHead(new Point(lineX.X2, lineX.Y2), new Point(1, 0), Colors.LightGray);
        canvas.DrawArrowHead(new Point(lineY.X1, lineY.Y1), new Point(0, 1), Colors.LightGray);

        canvas.DrawText("X", lineX.X2 - 20, lineX.Y2 + 2);
        canvas.DrawText("Y", lineY.X1 + 10, lineY.Y1 + 2);
        canvas.DrawText("0", center.X, center.Y);
    }

    public static void DrawText(this Canvas canvas, string text, double left, double top, double fontSize = 16)
    {
        TextBlock textBlock = new()
        {
            Text = text,
            Foreground = Brushes.Black,
            FontSize = fontSize,
            ToolTip = text,
        };

        Canvas.SetLeft(textBlock, left);
        Canvas.SetTop(textBlock, top);
        canvas.Children.Add(textBlock);
    }

    public static void DrawProjection(this Canvas canvas, Vector drawingVector, Color color, Point center, Vector vector)
    {
        Point start = center;
        Point end = new(start.X + drawingVector.X, start.Y - drawingVector.Y);

        DrawLine(end.X, end.Y, end.X, start.Y);
        DrawLabel(end.X, start.Y, vector.X);

        DrawLine(end.X, end.Y, start.X, end.Y);
        DrawLabel(start.X, end.Y, vector.Y);
        return;

        void DrawLine(double x1, double y1, double x2, double y2)
        {
            Line projectionLine = new()
            {
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 3,
                StrokeDashArray = [2, 2],
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
            };

            canvas.Children.Add(projectionLine);
        }

        void DrawLabel(double x, double y, double value)
        {
            TextBlock label = new()
            {
                Text = $"{value:F3}",
                FontSize = 16,
                Foreground = new SolidColorBrush(color),
                Background = new SolidColorBrush(Colors.White),
                ToolTip = $"{value:F3}",
            };

            Canvas.SetLeft(label, x + 2);
            Canvas.SetTop(label, y + 2);
            canvas.Children.Add(label);
        }
    }

    public static void DrawArrowHead(this Canvas canvas, Point center, Point vector, Color color)
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
                StrokeThickness = 4,
                X1 = center.X,
                Y1 = center.Y,
                X2 = end.X,
                Y2 = end.Y,
            };

            canvas.Children.Add(arrowLine);
        }
    }
}
