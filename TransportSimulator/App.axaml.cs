using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TEA;
using TransportSimulator.Views;

namespace TransportSimulatorAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            //desktop.MainWindow = new BepuTransport();
            // desktop.MainWindow = new MainWindow
            // {
            //     DataContext = new MainWindowViewModel(),
            // };


            var bufferDispatcher = new BufferDispatcher<ICircuitDesignerMessage>();
            var view = new CircuitDesignerView(bufferDispatcher);
            var tea = new TEA<CircuitDesignerModel, ICircuitDesignerMessage>(new CircuitDesignerModel(), view);
            bufferDispatcher.Setup(tea);
            var content = view;

            var logics = new StackPanel {
                Children = {
                    new AndView(),
                    new OrView(),
                    new NotView(),
                    new NandView(),
                    new XorView(),
                }
            };
            //var content = logics;

            desktop.MainWindow = new Window() {
                Width = 1250,
                Height = 650,
                Content = content
            };

        }

        base.OnFrameworkInitializationCompleted();
    }
}