using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Declarative;

namespace TransportSimulator.Views;

public class ButtonView : ComponentBase {
    protected override object Build() {
        return new Grid() {
            Margin = new Thickness(5),
            Children = {
                new Button()
            }
        };
    }
}