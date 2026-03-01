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
public static partial class GroupingNode_MarkupExtensions
{
//================= Properties ======================//
 // HeaderBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.HeaderBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.HeaderBrushProperty, ps, () => control.HeaderBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderBrush<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.HeaderBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.HeaderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.HeaderBrushProperty, ps, () => control.HeaderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanResizeProperty

/*BindFromExpressionSetterGenerator*/
public static T CanResize<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.CanResizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanResize<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.CanResizeProperty, ps, () => control.CanResize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanResize<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.CanResizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanResize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.CanResizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanResize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.CanResizeProperty, ps, () => control.CanResize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActualSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ActualSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ActualSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActualSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ActualSizeProperty, ps, () => control.ActualSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ActualSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.GroupingNode
   => control._set(() => control.ActualSize = new Avalonia.Size(width, height));
public static T ActualSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.GroupingNode
   => control._set(() => control.ActualSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ActualSize<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ActualSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActualSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ActualSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActualSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ActualSizeProperty, ps, () => control.ActualSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MovementModeProperty

/*BindFromExpressionSetterGenerator*/
public static T MovementMode<T>(this T control, Func<Nodify.GroupingMovementMode> func, Action<Nodify.GroupingMovementMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.MovementModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MovementMode<T>(this T control, Nodify.GroupingMovementMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.MovementModeProperty, ps, () => control.MovementMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MovementMode<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.MovementModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MovementMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.MovementModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MovementMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.GroupingMovementMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.MovementModeProperty, ps, () => control.MovementMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ResizeCompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ResizeCompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeCompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ResizeCompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ResizeCompletedCommandProperty, ps, () => control.ResizeCompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ResizeCompletedCommand<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeCompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ResizeCompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeCompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ResizeCompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ResizeCompletedCommandProperty, ps, () => control.ResizeCompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ResizeStartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ResizeStartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeStartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ResizeStartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ResizeStartedCommandProperty, ps, () => control.ResizeStartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ResizeStartedCommand<T>(this T control, IBinding binding) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeStartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ResizeStartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.GroupingNode
   => control._set(Nodify.GroupingNode.ResizeStartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ResizeStartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.GroupingNode
=> control._setEx(Nodify.GroupingNode.ResizeStartedCommandProperty, ps, () => control.ResizeStartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ResizeCompleted

/*ActionToEventGenerator*/
    public static T OnResizeCompleted<T>(this T control, Action<Nodify.ResizeEventArgs> action) where T : Nodify.GroupingNode => 
        control._setEvent((Nodify.ResizeEventHandler) ((arg0, arg1) => action(arg1)), h => control.ResizeCompleted += h);


 // ResizeStarted

/*ActionToEventGenerator*/
    public static T OnResizeStarted<T>(this T control, Action<Nodify.ResizeEventArgs> action) where T : Nodify.GroupingNode => 
        control._setEvent((Nodify.ResizeEventHandler) ((arg0, arg1) => action(arg1)), h => control.ResizeStarted += h);



//================= Styles ======================//
 // HeaderBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.HeaderBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.HeaderBrushProperty, binding);


 // CanResizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanResize<T>(this Style<T> style, System.Boolean value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.CanResizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanResize<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.CanResizeProperty, binding);


 // ActualSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ActualSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ActualSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.GroupingNode
   => style._addSetter(Nodify.GroupingNode.ActualSizeProperty, new Avalonia.Size(width, height));public static Style<T> ActualSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.GroupingNode
   => style._addSetter(Nodify.GroupingNode.ActualSizeProperty, new Avalonia.Size(vector2));


 // MovementModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MovementMode<T>(this Style<T> style, Nodify.GroupingMovementMode value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.MovementModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MovementMode<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.MovementModeProperty, binding);


 // ResizeCompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ResizeCompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ResizeCompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ResizeCompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ResizeCompletedCommandProperty, binding);


 // ResizeStartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ResizeStartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ResizeStartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ResizeStartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.GroupingNode
=> style._addSetter(Nodify.GroupingNode.ResizeStartedCommandProperty, binding);



}
