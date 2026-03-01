using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace TransportSimulator.Views;

public class NotView : ComponentBase {
    protected override object Build() {
        return new Grid {
            Margin = new Thickness(5),
            Children = {
                // 三角形本体
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 20 L 25 80 L 80 50 Z") },
                // バブル(反転)
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 90 50 m -5,0 a 5,5 0 1,0 10,0 a 5,5 0 1,0 -10,0") }
            }
        };
    }
}