using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;

namespace TransportSimulator.Views;

public static class FuncDataTemplateExtensions {
    public static DataTemplate ToDataTemplate<T>(this FuncDataTemplate<T> x) {
        Func<IServiceProvider, object?> create = _ => {
            var control = new UserControl();
            control.DataContextChanged += (_, _) => {
                var dataContext = control.DataContext;
                Console.WriteLine($"DataContext is changed. {control.DataContext?.ToString() ?? "null"}");
                control.Content = dataContext == null
                    ? null
                    : x.Build(dataContext);
            };
            return new TemplateResult<Control>(control, new NameScope());
        };
        return new DataTemplate() { DataType = typeof(T), Content = create };
    }
}