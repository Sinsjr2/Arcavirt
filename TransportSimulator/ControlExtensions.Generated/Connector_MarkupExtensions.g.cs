#nullable enable
using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Avalonia.Markup.Declarative;
[global::System.CodeDom.Compiler.GeneratedCode("AvaloniaExtensionGenerator", "11.1.3.0")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public static partial class Connector_MarkupExtensions
{
//================= Properties ======================//
 // HorizontalContentAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Func<Avalonia.Layout.HorizontalAlignment> func, Action<Avalonia.Layout.HorizontalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.HorizontalContentAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, Avalonia.Layout.HorizontalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, IBinding binding) where T : Nodify.Connector
   => control._set(Nodify.Connector.HorizontalContentAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HorizontalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.HorizontalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HorizontalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.HorizontalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.HorizontalContentAlignmentProperty, ps, () => control.HorizontalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // VerticalContentAlignmentProperty

/*BindFromExpressionSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, Func<Avalonia.Layout.VerticalAlignment> func, Action<Avalonia.Layout.VerticalAlignment>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.VerticalContentAlignmentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, Avalonia.Layout.VerticalAlignment value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, IBinding binding) where T : Nodify.Connector
   => control._set(Nodify.Connector.VerticalContentAlignmentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T VerticalContentAlignment<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.VerticalContentAlignmentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T VerticalContentAlignment<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.VerticalAlignment> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.VerticalContentAlignmentProperty, ps, () => control.VerticalContentAlignment = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AnchorProperty

/*BindFromExpressionSetterGenerator*/
public static T Anchor<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.AnchorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Anchor<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.AnchorProperty, ps, () => control.Anchor = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Anchor<T>(this T control, IBinding binding) where T : Nodify.Connector
   => control._set(Nodify.Connector.AnchorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Anchor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.AnchorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Anchor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.AnchorProperty, ps, () => control.Anchor = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsConnectedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsConnected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.IsConnectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsConnected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.IsConnectedProperty, ps, () => control.IsConnected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsConnected<T>(this T control, IBinding binding) where T : Nodify.Connector
   => control._set(Nodify.Connector.IsConnectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsConnected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.IsConnectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsConnected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.IsConnectedProperty, ps, () => control.IsConnected = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisconnectCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T DisconnectCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.DisconnectCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisconnectCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.DisconnectCommandProperty, ps, () => control.DisconnectCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisconnectCommand<T>(this T control, IBinding binding) where T : Nodify.Connector
   => control._set(Nodify.Connector.DisconnectCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisconnectCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Connector
   => control._set(Nodify.Connector.DisconnectCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisconnectCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Connector
=> control._setEx(Nodify.Connector.DisconnectCommandProperty, ps, () => control.DisconnectCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // PendingConnectionStarted

/*ActionToEventGenerator*/
    public static T OnPendingConnectionStarted<T>(this T control, Action<Nodify.PendingConnectionEventArgs> action) where T : Nodify.Connector => 
        control._setEvent((Nodify.PendingConnectionEventHandler) ((arg0, arg1) => action(arg1)), h => control.PendingConnectionStarted += h);


 // PendingConnectionCompleted

/*ActionToEventGenerator*/
    public static T OnPendingConnectionCompleted<T>(this T control, Action<Nodify.PendingConnectionEventArgs> action) where T : Nodify.Connector => 
        control._setEvent((Nodify.PendingConnectionEventHandler) ((arg0, arg1) => action(arg1)), h => control.PendingConnectionCompleted += h);


 // PendingConnectionDrag

/*ActionToEventGenerator*/
    public static T OnPendingConnectionDrag<T>(this T control, Action<Nodify.PendingConnectionEventArgs> action) where T : Nodify.Connector => 
        control._setEvent((Nodify.PendingConnectionEventHandler) ((arg0, arg1) => action(arg1)), h => control.PendingConnectionDrag += h);


 // Disconnect

/*ActionToEventGenerator*/
    public static T OnDisconnect<T>(this T control, Action<Nodify.ConnectorEventArgs> action) where T : Nodify.Connector => 
        control._setEvent((Nodify.ConnectorEventHandler) ((arg0, arg1) => action(arg1)), h => control.Disconnect += h);



//================= Styles ======================//
 // HorizontalContentAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, Avalonia.Layout.HorizontalAlignment value) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.HorizontalContentAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HorizontalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.HorizontalContentAlignmentProperty, binding);


 // VerticalContentAlignmentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, Avalonia.Layout.VerticalAlignment value) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.VerticalContentAlignmentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> VerticalContentAlignment<T>(this Style<T> style, IBinding binding) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.VerticalContentAlignmentProperty, binding);


 // AnchorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Anchor<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.AnchorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Anchor<T>(this Style<T> style, IBinding binding) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.AnchorProperty, binding);


 // IsConnectedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsConnected<T>(this Style<T> style, System.Boolean value) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.IsConnectedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsConnected<T>(this Style<T> style, IBinding binding) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.IsConnectedProperty, binding);


 // DisconnectCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisconnectCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.DisconnectCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisconnectCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.Connector
=> style._addSetter(Nodify.Connector.DisconnectCommandProperty, binding);



}
