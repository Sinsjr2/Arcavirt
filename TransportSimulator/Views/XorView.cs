using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace TransportSimulator.Views;

public class XorView : ComponentBase {
    protected override object Build() {
        return new Grid {
            Margin = new Thickness(5),
            Children = {
                // ORの背面より少し左に平行カーブ
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 20 20 Q 35 50 20 80") },

                // ORの背面カーブ -->
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 20 Q 40 50 25 80") },
                // 上辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 20 Q 55 20 80 50") },
                // 下辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 25 80 Q 55 80 80 50") },
            }
        };
    }
}