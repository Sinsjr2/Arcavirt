using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using BepuPhysics;

namespace TransportSimulatorAvalonia.Views;

public class RollerView {
    readonly Bodies bodies;
    readonly BodyHandle bodyHandle;
    readonly Canvas canvas;

    public RollerView(Canvas canvas, Bodies bodies, BodyHandle bodyHandle) {
        this.canvas = canvas;
        this.bodies = bodies;
        this.bodyHandle = bodyHandle;
    }

    public void Render(int displaySize, Vector position, float rotationAngle) {
        var rollerDispSize = displaySize;
        var rollerGrid = new Grid();
        var rollerCircle = new Ellipse() {
            Width = rollerDispSize,
            Height = rollerDispSize,
            Stroke = Brushes.Black,
            StrokeThickness = 2,
            Margin = new Thickness(-(rollerDispSize / 2), -(rollerDispSize / 2))
        };
        var line1 = new Line() {
            Stroke = Brushes.Black,
            StartPoint = new Point(rollerDispSize / 2, 0),
            EndPoint = new(0, rollerDispSize / 2)
        };
        rollerGrid.Children.Add(rollerCircle);
        rollerGrid.Children.Add(line1);
        canvas.Children.Add(rollerGrid);
        rollerGrid.RenderTransform = new RotateTransform() { Angle = rotationAngle };
        Canvas.SetLeft(rollerGrid, position.X);
        Canvas.SetTop(rollerGrid, position.Y);
    }
}