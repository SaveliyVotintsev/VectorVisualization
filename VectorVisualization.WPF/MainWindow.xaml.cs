﻿using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
        Draw();
    }

    private void Draw()
    {
        try
        {
            double radius = _diameter / 2;
            Vector drawingVector1 = _vector1 * radius;
            Vector drawingVector2 = _vector2 * radius;

            DrawingCanvas.Children.Clear();

            DrawingCanvas.DrawAxes(_center);

            string label = _vectorLenght.ToString("F3");

            DrawingCanvas.DrawText(label, _center.X + Radius, _center.Y, 10);
            DrawingCanvas.DrawText(label, _center.X, _center.Y - Radius - 15, 10);
            DrawingCanvas.DrawText("-" + label, _center.X - Radius - 32, _center.Y, 10);
            DrawingCanvas.DrawText("-" + label, _center.X, _center.Y + Radius, 10);

            DrawingCanvas.DrawCircle(_diameter, _center);

            Color vectorColor1 = _draggingVector1 ? Colors.LightPink : Colors.Red;
            Color vectorColor2 = _draggingVector2 ? Colors.LightBlue : Colors.Blue;
            DrawingCanvas.DrawVector(drawingVector1, vectorColor1, _center);
            DrawingCanvas.DrawVector(drawingVector2, vectorColor2, _center);

            DrawingCanvas.DrawCircleCenter(_center);

            Vector vector1 = _vector1 * _vectorLenght;
            Vector vector2 = _vector2 * _vectorLenght;

            DrawingCanvas.DrawProjection(drawingVector1, vectorColor1, _center, vector1);
            DrawingCanvas.DrawProjection(drawingVector2, vectorColor2, _center, vector2);

            DrawingCanvas.DrawAngleArc(_vector1, _vector2, Colors.Green, _center, _diameter);

            UpdateInformation(vector1, vector2);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

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
