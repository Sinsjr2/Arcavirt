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
public static partial class PendingConnection_MarkupExtensions
{
//================= Properties ======================//
 // SourceAnchorProperty

/*BindFromExpressionSetterGenerator*/
public static T SourceAnchor<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceAnchorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourceAnchor<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.SourceAnchorProperty, ps, () => control.SourceAnchor = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SourceAnchor<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceAnchorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourceAnchor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceAnchorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourceAnchor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.SourceAnchorProperty, ps, () => control.SourceAnchor = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetAnchorProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetAnchor<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetAnchorProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetAnchor<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.TargetAnchorProperty, ps, () => control.TargetAnchor = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetAnchor<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetAnchorProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetAnchor<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetAnchorProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetAnchor<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.TargetAnchorProperty, ps, () => control.TargetAnchor = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceProperty

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Source<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetProperty

/*BindFromExpressionSetterGenerator*/
public static T Target<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Target<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Target<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PreviewTargetProperty

/*BindFromExpressionSetterGenerator*/
public static T PreviewTarget<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.PreviewTargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PreviewTarget<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.PreviewTargetProperty, ps, () => control.PreviewTarget = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PreviewTarget<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.PreviewTargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PreviewTarget<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.PreviewTargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PreviewTarget<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.PreviewTargetProperty, ps, () => control.PreviewTarget = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnablePreviewProperty

/*BindFromExpressionSetterGenerator*/
public static T EnablePreview<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnablePreviewProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnablePreview<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.EnablePreviewProperty, ps, () => control.EnablePreview = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnablePreview<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnablePreviewProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnablePreview<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnablePreviewProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnablePreview<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.EnablePreviewProperty, ps, () => control.EnablePreview = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StrokeThicknessProperty

/*BindFromExpressionSetterGenerator*/
public static T StrokeThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StrokeThickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeThicknessProperty, ps, () => control.StrokeThickness = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StrokeThickness<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StrokeThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StrokeThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeThicknessProperty, ps, () => control.StrokeThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StrokeDashArrayProperty

/*BindFromExpressionSetterGenerator*/
public static T StrokeDashArray<T>(this T control, Func<Avalonia.Collections.AvaloniaList<System.Double>?> func, Action<Avalonia.Collections.AvaloniaList<System.Double>?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeDashArrayProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StrokeDashArray<T>(this T control, Avalonia.Collections.AvaloniaList<System.Double> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeDashArrayProperty, ps, () => control.StrokeDashArray = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StrokeDashArray<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeDashArrayProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StrokeDashArray<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeDashArrayProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StrokeDashArray<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Collections.AvaloniaList<System.Double>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeDashArrayProperty, ps, () => control.StrokeDashArray = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StrokeProperty

/*BindFromExpressionSetterGenerator*/
public static T Stroke<T>(this T control, Func<Avalonia.Media.IBrush?> func, Action<Avalonia.Media.IBrush?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Stroke<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeProperty, ps, () => control.Stroke = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Stroke<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Stroke<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StrokeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Stroke<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StrokeProperty, ps, () => control.Stroke = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AllowOnlyConnectorsProperty

/*BindFromExpressionSetterGenerator*/
public static T AllowOnlyConnectors<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.AllowOnlyConnectorsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AllowOnlyConnectors<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.AllowOnlyConnectorsProperty, ps, () => control.AllowOnlyConnectors = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AllowOnlyConnectors<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.AllowOnlyConnectorsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AllowOnlyConnectors<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.AllowOnlyConnectorsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AllowOnlyConnectors<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.AllowOnlyConnectorsProperty, ps, () => control.AllowOnlyConnectors = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableSnappingProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableSnapping<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnableSnappingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableSnapping<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.EnableSnappingProperty, ps, () => control.EnableSnapping = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableSnapping<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnableSnappingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableSnapping<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.EnableSnappingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableSnapping<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.EnableSnappingProperty, ps, () => control.EnableSnapping = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DirectionProperty

/*BindFromExpressionSetterGenerator*/
public static T Direction<T>(this T control, Func<Nodify.ConnectionDirection> func, Action<Nodify.ConnectionDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.DirectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Direction<T>(this T control, Nodify.ConnectionDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.DirectionProperty, ps, () => control.Direction = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Direction<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.DirectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Direction<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.DirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Direction<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectionDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.DirectionProperty, ps, () => control.Direction = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // StartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T StartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StartedCommandProperty, ps, () => control.StartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StartedCommand<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.StartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.StartedCommandProperty, ps, () => control.StartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T CompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.CompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.CompletedCommandProperty, ps, () => control.CompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CompletedCommand<T>(this T control, IBinding binding) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.CompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.PendingConnection
   => control._set(Nodify.PendingConnection.CompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.PendingConnection
=> control._setEx(Nodify.PendingConnection.CompletedCommandProperty, ps, () => control.CompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // SourceAnchorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SourceAnchor<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.SourceAnchorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SourceAnchor<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.SourceAnchorProperty, binding);


 // TargetAnchorProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TargetAnchor<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.TargetAnchorProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetAnchor<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.TargetAnchorProperty, binding);


 // SourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, System.Object value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.SourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.SourceProperty, binding);


 // TargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, System.Object value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.TargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.TargetProperty, binding);


 // PreviewTargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PreviewTarget<T>(this Style<T> style, System.Object value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.PreviewTargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PreviewTarget<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.PreviewTargetProperty, binding);


 // EnablePreviewProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnablePreview<T>(this Style<T> style, System.Boolean value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.EnablePreviewProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnablePreview<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.EnablePreviewProperty, binding);


 // StrokeThicknessProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StrokeThickness<T>(this Style<T> style, System.Double value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StrokeThickness<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeThicknessProperty, binding);


 // StrokeDashArrayProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StrokeDashArray<T>(this Style<T> style, Avalonia.Collections.AvaloniaList<System.Double> value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeDashArrayProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StrokeDashArray<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeDashArrayProperty, binding);


 // StrokeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Stroke<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Stroke<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StrokeProperty, binding);


 // AllowOnlyConnectorsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AllowOnlyConnectors<T>(this Style<T> style, System.Boolean value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.AllowOnlyConnectorsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AllowOnlyConnectors<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.AllowOnlyConnectorsProperty, binding);


 // EnableSnappingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnableSnapping<T>(this Style<T> style, System.Boolean value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.EnableSnappingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableSnapping<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.EnableSnappingProperty, binding);


 // DirectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Direction<T>(this Style<T> style, Nodify.ConnectionDirection value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.DirectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Direction<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.DirectionProperty, binding);


 // IsVisibleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsVisible<T>(this Style<T> style, System.Boolean value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.IsVisibleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsVisible<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.IsVisibleProperty, binding);


 // StartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.StartedCommandProperty, binding);


 // CompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.CompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.PendingConnection
=> style._addSetter(Nodify.PendingConnection.CompletedCommandProperty, binding);



}
