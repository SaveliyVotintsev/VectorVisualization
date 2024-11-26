namespace WinFormsApp1;

public partial class VectorVisualization : Form
{
    private PointF _vector1 = new(1, 0);
    private PointF _vector2 = new(0, 1);
    private bool _draggingVector1;
    private bool _draggingVector2;

    public VectorVisualization()
    {
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        Paint += OnPaint;
    }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        if (IsPointOnVector(e.Location, _vector1))
        {
            _draggingVector1 = true;
        }
        else if (IsPointOnVector(e.Location, _vector2))
        {
            _draggingVector2 = true;
        }
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_draggingVector1)
        {
            _vector1 = GetUnitVector(e.Location);
            Invalidate();
        }
        else if (_draggingVector2)
        {
            _vector2 = GetUnitVector(e.Location);
            Invalidate();
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        _draggingVector1 = false;
        _draggingVector2 = false;
    }

    private void OnPaint(object? sender, PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Color.White);
        DrawCircle(g);
        DrawVector(g, _vector1, Color.Red);
        DrawVector(g, _vector2, Color.Blue);
        DrawDotProduct(g);
        DrawCoordinates(g);
        DrawAngle(g);
    }

    private void DrawCircle(Graphics g)
    {
        g.DrawEllipse(Pens.Black, 300, 200, 200, 200);
    }

    private void DrawVector(Graphics g, PointF vector, Color color)
    {
        PointF start = new(400, 300);
        PointF end = new(400 + vector.X * 100, 300 - vector.Y * 100);
        g.DrawLine(new Pen(color, 2), start, end);
        DrawArrowHead(g, end, vector, color);
    }

    private void DrawArrowHead(Graphics g, PointF end, PointF vector, Color color)
    {
        float arrowLength = 10;
        float arrowAngle = (float)(Math.PI / 6); // 30 degrees

        float angle = (float)Math.Atan2(vector.Y, vector.X);

        PointF arrowPoint1 = new(end.X - arrowLength * (float)Math.Cos(angle - arrowAngle),
            end.Y - arrowLength * (float)Math.Sin(angle - arrowAngle));

        PointF arrowPoint2 = new(end.X - arrowLength * (float)Math.Cos(angle + arrowAngle),
            end.Y - arrowLength * (float)Math.Sin(angle + arrowAngle));

        g.DrawLine(new Pen(color, 2), end, arrowPoint1);
        g.DrawLine(new Pen(color, 2), end, arrowPoint2);
    }

    private void DrawDotProduct(Graphics g)
    {
        float dotProduct = _vector1.X * _vector2.X + _vector1.Y * _vector2.Y;
        g.DrawString($"Скалярное произведение: {dotProduct:F2}", Font, Brushes.Black, 10, 10);
    }

    private void DrawCoordinates(Graphics g)
    {
        g.DrawString($"Координаты вектора 1: ({_vector1.X:F2}, {_vector1.Y:F2})", Font, Brushes.Red, 10, 30);
        g.DrawString($"Координаты вектора 2: ({_vector2.X:F2}, {_vector2.Y:F2})", Font, Brushes.Blue, 10, 50);
    }

    private void DrawAngle(Graphics g)
    {
        float dotProduct = _vector1.X * _vector2.X + _vector1.Y * _vector2.Y;
        float angle = (float)(Math.Acos(dotProduct) * (180.0 / Math.PI)); // Угол в градусах
        g.DrawString($"Угол между векторами: {angle:F2}°", Font, Brushes.Black, 10, 70);
    }

    private bool IsPointOnVector(PointF point, PointF vector)
    {
        float x = 400 + vector.X * 100;
        float y = 300 - vector.Y * 100;
        return Math.Abs(point.X - x) < 10 && Math.Abs(point.Y - y) < 10;
    }

    private PointF GetUnitVector(PointF point)
    {
        float centerX = 400;
        float centerY = 300;
        float dx = point.X - centerX;
        float dy = centerY - point.Y;
        float length = (float)Math.Sqrt(dx * dx + dy * dy);
        return new PointF(dx / length, dy / length);
    }
}
