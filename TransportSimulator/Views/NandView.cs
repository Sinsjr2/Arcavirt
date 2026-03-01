
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Declarative;
using Avalonia.Media;

namespace TransportSimulator.Views;

public class NandView : ComponentBase {
    protected override object Build() {
        return new Grid() {
            Margin = new Thickness(5),
            Children = {
                // 本体(角丸矩形左+半円右)
                // 左側縦
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 30 20 L 30 80") },
                //上辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 30 20 L 60 20") },
                // 下辺
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 30 80 L 60 80") },
                // 半円(右側)
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 60 20 A 30 30 0 0 1 60 80") } ,
                // バブル
                new Path { Stroke = Brushes.Black, StrokeThickness = 3, Data = Geometry.Parse("M 97 50 m -5,0 a 5,5 0 1,0 10,0 a 5,5 0 1,0 -10,0") }
            }
        };
    }
}