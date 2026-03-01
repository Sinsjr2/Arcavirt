using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace TransportSimulator.Views;

public class NorView : ComponentBase {
    protected override object Build() {
        return new Grid {
            Margin = new Thickness(5),
            Children = {
                // ORの背面カーブ -->
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 20 Q 40 50 25 80") },
                // 上辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 20 Q 55 20 80 50") },
                // 下辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 80 Q 55 80 80 50") },

                // バブル
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 85 50 m -5,0 a 5,5 0 1,0 10,0 a 5,5 0 1,0 -10,0") }
            }
        };
    }
}