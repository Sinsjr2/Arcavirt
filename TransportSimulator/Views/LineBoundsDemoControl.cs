using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Threading;

namespace TransportSimulatorAvalonia.Views
{
    public class LineBoundsDemoControl : Control
    {
        static LineBoundsDemoControl()
        {
            AffectsRender<LineBoundsDemoControl>(AngleProperty);
        }

        public LineBoundsDemoControl()
        {
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1 / 60.0);
            timer.Tick += (sender, e) => Angle += Math.PI / 360;
            timer.Start();
        }

        public static readonly StyledProperty<double> AngleProperty =
            AvaloniaProperty.Register<LineBoundsDemoControl, double>(nameof(Angle));

        public double Angle
        {
            get => GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        public override void Render(DrawingContext drawingContext)
        {
            var path = new Point[] {
                new(0, 40),
                new(50, 40),
                new(70, 60)
            };
            var lineLength = Math.Sqrt((100 * 100) + (100 * 100));

            var diffX = 200;//LineBoundsHelper.CalculateAdjSide(Angle, lineLength);
            var diffY = 10;//LineBoundsHelper.CalculateOppSide(Angle, lineLength);


            var p1 = new Point(200, 200);
            var p2 = new Point(p1.X + diffX, p1.Y + diffY);

            var pen = new Pen(Brushes.Green, 20, lineCap: PenLineCap.Square);
            var boundPen = new Pen(Brushes.Black);

            // drawingContext.DrawLine(pen, p1, p2);

            for (int i = 1; i < path.Length; i++) {
                drawingContext.DrawLine(pen, path[i - 1], path[i]);
            }

            //drawingContext.DrawRectangle(boundPen, LineBoundsHelper.CalculateBounds(p1, p2, pen));
        }
    }
}
