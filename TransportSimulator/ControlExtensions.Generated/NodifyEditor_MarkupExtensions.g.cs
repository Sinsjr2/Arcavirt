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
public static partial class NodifyEditor_MarkupExtensions
{
//================= Properties ======================//
 // ViewportZoomProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportZoom<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportZoomProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportZoom<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportZoomProperty, ps, () => control.ViewportZoom = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ViewportZoom<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportZoomProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportZoom<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportZoomProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportZoom<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportZoomProperty, ps, () => control.ViewportZoom = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MinViewportZoomProperty

/*BindFromExpressionSetterGenerator*/
public static T MinViewportZoom<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MinViewportZoomProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MinViewportZoom<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.MinViewportZoomProperty, ps, () => control.MinViewportZoom = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MinViewportZoom<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MinViewportZoomProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MinViewportZoom<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MinViewportZoomProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MinViewportZoom<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.MinViewportZoomProperty, ps, () => control.MinViewportZoom = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxViewportZoomProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxViewportZoom<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MaxViewportZoomProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxViewportZoom<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.MaxViewportZoomProperty, ps, () => control.MaxViewportZoom = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T MaxViewportZoom<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MaxViewportZoomProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxViewportZoom<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.MaxViewportZoomProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxViewportZoom<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.MaxViewportZoomProperty, ps, () => control.MaxViewportZoom = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ViewportLocationProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportLocation<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportLocationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportLocation<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportLocationProperty, ps, () => control.ViewportLocation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ViewportLocation<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportLocationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportLocation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportLocationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportLocation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportLocationProperty, ps, () => control.ViewportLocation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ViewportSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportSizeProperty, ps, () => control.ViewportSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ViewportSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ViewportSize = new Avalonia.Size(width, height));
public static T ViewportSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ViewportSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ViewportSize<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportSizeProperty, ps, () => control.ViewportSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsExtentProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsExtent<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsExtentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsExtent<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsExtentProperty, ps, () => control.ItemsExtent = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ItemsExtent<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(x, y, width, height));
public static T ItemsExtent<T>(this T control, Avalonia.Size size = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(size));
public static T ItemsExtent<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(position, size));
public static T ItemsExtent<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : Nodify.NodifyEditor
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T ItemsExtent<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsExtentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsExtent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsExtentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsExtent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsExtentProperty, ps, () => control.ItemsExtent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DecoratorsExtentProperty

/*BindFromExpressionSetterGenerator*/
public static T DecoratorsExtent<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsExtentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DecoratorsExtent<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorsExtentProperty, ps, () => control.DecoratorsExtent = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T DecoratorsExtent<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : Nodify.NodifyEditor
   => control._set(() => control.DecoratorsExtent = new Avalonia.Rect(x, y, width, height));
public static T DecoratorsExtent<T>(this T control, Avalonia.Size size = default) where T : Nodify.NodifyEditor
   => control._set(() => control.DecoratorsExtent = new Avalonia.Rect(size));
public static T DecoratorsExtent<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : Nodify.NodifyEditor
   => control._set(() => control.DecoratorsExtent = new Avalonia.Rect(position, size));
public static T DecoratorsExtent<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : Nodify.NodifyEditor
   => control._set(() => control.DecoratorsExtent = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T DecoratorsExtent<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsExtentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DecoratorsExtent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsExtentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DecoratorsExtent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorsExtentProperty, ps, () => control.DecoratorsExtent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ViewportTransformProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportTransform<T>(this T control, Func<Avalonia.Media.Transform> func, Action<Avalonia.Media.Transform>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportTransformProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportTransform<T>(this T control, Avalonia.Media.Transform value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportTransformProperty, ps, () => control.ViewportTransform = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ViewportTransform<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportTransformProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportTransform<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ViewportTransformProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportTransform<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Transform> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ViewportTransformProperty, ps, () => control.ViewportTransform = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DpiScaledViewportTransformProperty

/*BindFromExpressionSetterGenerator*/
public static T DpiScaledViewportTransform<T>(this T control, Func<Avalonia.Media.Transform> func, Action<Avalonia.Media.Transform>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DpiScaledViewportTransform<T>(this T control, Avalonia.Media.Transform value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, ps, () => control.DpiScaledViewportTransform = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DpiScaledViewportTransform<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DpiScaledViewportTransform<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DpiScaledViewportTransform<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.Transform> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, ps, () => control.DpiScaledViewportTransform = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BringIntoViewSpeedProperty

/*BindFromExpressionSetterGenerator*/
public static T BringIntoViewSpeed<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewSpeedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BringIntoViewSpeed<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.BringIntoViewSpeedProperty, ps, () => control.BringIntoViewSpeed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BringIntoViewSpeed<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewSpeedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BringIntoViewSpeed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewSpeedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BringIntoViewSpeed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.BringIntoViewSpeedProperty, ps, () => control.BringIntoViewSpeed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // BringIntoViewMaxDurationProperty

/*BindFromExpressionSetterGenerator*/
public static T BringIntoViewMaxDuration<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T BringIntoViewMaxDuration<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, ps, () => control.BringIntoViewMaxDuration = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T BringIntoViewMaxDuration<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T BringIntoViewMaxDuration<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T BringIntoViewMaxDuration<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, ps, () => control.BringIntoViewMaxDuration = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisplayConnectionsOnTopProperty

/*BindFromExpressionSetterGenerator*/
public static T DisplayConnectionsOnTop<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisplayConnectionsOnTop<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, ps, () => control.DisplayConnectionsOnTop = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisplayConnectionsOnTop<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisplayConnectionsOnTop<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisplayConnectionsOnTop<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, ps, () => control.DisplayConnectionsOnTop = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisableAutoPanningProperty

/*BindFromExpressionSetterGenerator*/
public static T DisableAutoPanning<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableAutoPanningProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisableAutoPanning<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisableAutoPanningProperty, ps, () => control.DisableAutoPanning = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisableAutoPanning<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableAutoPanningProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisableAutoPanning<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableAutoPanningProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisableAutoPanning<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisableAutoPanningProperty, ps, () => control.DisableAutoPanning = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AutoPanSpeedProperty

/*BindFromExpressionSetterGenerator*/
public static T AutoPanSpeed<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanSpeedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AutoPanSpeed<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.AutoPanSpeedProperty, ps, () => control.AutoPanSpeed = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AutoPanSpeed<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanSpeedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AutoPanSpeed<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanSpeedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AutoPanSpeed<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.AutoPanSpeedProperty, ps, () => control.AutoPanSpeed = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // AutoPanEdgeDistanceProperty

/*BindFromExpressionSetterGenerator*/
public static T AutoPanEdgeDistance<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T AutoPanEdgeDistance<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, ps, () => control.AutoPanEdgeDistance = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T AutoPanEdgeDistance<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T AutoPanEdgeDistance<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T AutoPanEdgeDistance<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, ps, () => control.AutoPanEdgeDistance = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ConnectionTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T ConnectionTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ConnectionTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionTemplateProperty, ps, () => control.ConnectionTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ConnectionTemplate<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ConnectionTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ConnectionTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionTemplateProperty, ps, () => control.ConnectionTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DecoratorTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T DecoratorTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DecoratorTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorTemplateProperty, ps, () => control.DecoratorTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DecoratorTemplate<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DecoratorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DecoratorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorTemplateProperty, ps, () => control.DecoratorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PendingConnectionTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T PendingConnectionTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PendingConnectionTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.PendingConnectionTemplateProperty, ps, () => control.PendingConnectionTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PendingConnectionTemplate<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PendingConnectionTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PendingConnectionTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.PendingConnectionTemplateProperty, ps, () => control.PendingConnectionTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectionRectangleStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectionRectangleStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectionRectangleStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectionRectangleStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectionRectangleStyleProperty, ps, () => control.SelectionRectangleStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectionRectangleStyle<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectionRectangleStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectionRectangleStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectionRectangleStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectionRectangleStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectionRectangleStyleProperty, ps, () => control.SelectionRectangleStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CuttingLineStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T CuttingLineStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingLineStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CuttingLineStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingLineStyleProperty, ps, () => control.CuttingLineStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CuttingLineStyle<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingLineStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CuttingLineStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingLineStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CuttingLineStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingLineStyleProperty, ps, () => control.CuttingLineStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DecoratorContainerStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T DecoratorContainerStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorContainerStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DecoratorContainerStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorContainerStyleProperty, ps, () => control.DecoratorContainerStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DecoratorContainerStyle<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorContainerStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DecoratorContainerStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorContainerStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DecoratorContainerStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorContainerStyleProperty, ps, () => control.DecoratorContainerStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ConnectionsProperty

/*BindFromExpressionSetterGenerator*/
public static T Connections<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Connections<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionsProperty, ps, () => control.Connections = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Connections<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Connections<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Connections<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionsProperty, ps, () => control.Connections = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedItems<T>(this T control, Func<System.Collections.IList> func, Action<System.Collections.IList>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedItems<T>(this T control, System.Collections.IList value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedItemsProperty, ps, () => control.SelectedItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedItems<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IList> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedItemsProperty, ps, () => control.SelectedItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedConnectionsProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedConnections<T>(this T control, Func<System.Collections.IList> func, Action<System.Collections.IList>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedConnections<T>(this T control, System.Collections.IList value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedConnectionsProperty, ps, () => control.SelectedConnections = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedConnections<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedConnections<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedConnections<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IList> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedConnectionsProperty, ps, () => control.SelectedConnections = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedConnectionProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedConnection<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedConnection<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedConnectionProperty, ps, () => control.SelectedConnection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedConnection<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedConnection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.SelectedConnectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedConnection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.SelectedConnectionProperty, ps, () => control.SelectedConnection = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // PendingConnectionProperty

/*BindFromExpressionSetterGenerator*/
public static T PendingConnection<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T PendingConnection<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.PendingConnectionProperty, ps, () => control.PendingConnection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T PendingConnection<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T PendingConnection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.PendingConnectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T PendingConnection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.PendingConnectionProperty, ps, () => control.PendingConnection = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // GridCellSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T GridCellSize<T>(this T control, Func<System.UInt32> func, Action<System.UInt32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.GridCellSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T GridCellSize<T>(this T control, System.UInt32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.GridCellSizeProperty, ps, () => control.GridCellSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T GridCellSize<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.GridCellSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T GridCellSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.GridCellSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T GridCellSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.UInt32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.GridCellSizeProperty, ps, () => control.GridCellSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisableZoomingProperty

/*BindFromExpressionSetterGenerator*/
public static T DisableZooming<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableZoomingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisableZooming<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisableZoomingProperty, ps, () => control.DisableZooming = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisableZooming<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableZoomingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisableZooming<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisableZoomingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisableZooming<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisableZoomingProperty, ps, () => control.DisableZooming = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisablePanningProperty

/*BindFromExpressionSetterGenerator*/
public static T DisablePanning<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisablePanningProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisablePanning<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisablePanningProperty, ps, () => control.DisablePanning = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisablePanning<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisablePanningProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisablePanning<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisablePanningProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisablePanning<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisablePanningProperty, ps, () => control.DisablePanning = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EnableRealtimeSelectionProperty

/*BindFromExpressionSetterGenerator*/
public static T EnableRealtimeSelection<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EnableRealtimeSelection<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, ps, () => control.EnableRealtimeSelection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EnableRealtimeSelection<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EnableRealtimeSelection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EnableRealtimeSelection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, ps, () => control.EnableRealtimeSelection = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DecoratorsProperty

/*BindFromExpressionSetterGenerator*/
public static T Decorators<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Decorators<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorsProperty, ps, () => control.Decorators = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Decorators<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Decorators<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DecoratorsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Decorators<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DecoratorsProperty, ps, () => control.Decorators = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanSelectMultipleConnectionsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanSelectMultipleConnections<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanSelectMultipleConnections<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, ps, () => control.CanSelectMultipleConnections = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanSelectMultipleConnections<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanSelectMultipleConnections<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanSelectMultipleConnections<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, ps, () => control.CanSelectMultipleConnections = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CanSelectMultipleItemsProperty

/*BindFromExpressionSetterGenerator*/
public static T CanSelectMultipleItems<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CanSelectMultipleItems<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, ps, () => control.CanSelectMultipleItems = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CanSelectMultipleItems<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CanSelectMultipleItems<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CanSelectMultipleItems<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, ps, () => control.CanSelectMultipleItems = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ConnectionCompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ConnectionCompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ConnectionCompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, ps, () => control.ConnectionCompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ConnectionCompletedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ConnectionCompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ConnectionCompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, ps, () => control.ConnectionCompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ConnectionStartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ConnectionStartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionStartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ConnectionStartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionStartedCommandProperty, ps, () => control.ConnectionStartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ConnectionStartedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionStartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ConnectionStartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ConnectionStartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ConnectionStartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ConnectionStartedCommandProperty, ps, () => control.ConnectionStartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisconnectConnectorCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T DisconnectConnectorCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisconnectConnectorCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, ps, () => control.DisconnectConnectorCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisconnectConnectorCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisconnectConnectorCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisconnectConnectorCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, ps, () => control.DisconnectConnectorCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // RemoveConnectionCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T RemoveConnectionCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.RemoveConnectionCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T RemoveConnectionCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.RemoveConnectionCommandProperty, ps, () => control.RemoveConnectionCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T RemoveConnectionCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.RemoveConnectionCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T RemoveConnectionCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.RemoveConnectionCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T RemoveConnectionCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.RemoveConnectionCommandProperty, ps, () => control.RemoveConnectionCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsDragStartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsDragStartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsDragStartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, ps, () => control.ItemsDragStartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsDragStartedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsDragStartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsDragStartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, ps, () => control.ItemsDragStartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsDragCompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsDragCompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsDragCompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, ps, () => control.ItemsDragCompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsDragCompletedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsDragCompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsDragCompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, ps, () => control.ItemsDragCompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsSelectStartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsSelectStartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSelectStartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, ps, () => control.ItemsSelectStartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSelectStartedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSelectStartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSelectStartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, ps, () => control.ItemsSelectStartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsSelectCompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsSelectCompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsSelectCompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, ps, () => control.ItemsSelectCompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ItemsSelectCompletedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsSelectCompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsSelectCompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, ps, () => control.ItemsSelectCompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CuttingStartedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T CuttingStartedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingStartedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CuttingStartedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingStartedCommandProperty, ps, () => control.CuttingStartedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CuttingStartedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingStartedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CuttingStartedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingStartedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CuttingStartedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingStartedCommandProperty, ps, () => control.CuttingStartedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // CuttingCompletedCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T CuttingCompletedCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingCompletedCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T CuttingCompletedCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingCompletedCommandProperty, ps, () => control.CuttingCompletedCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T CuttingCompletedCommand<T>(this T control, IBinding binding) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingCompletedCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T CuttingCompletedCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyEditor
   => control._set(Nodify.NodifyEditor.CuttingCompletedCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T CuttingCompletedCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyEditor
=> control._setEx(Nodify.NodifyEditor.CuttingCompletedCommandProperty, ps, () => control.CuttingCompletedCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // ScrollInvalidated

/*ActionToEventGenerator*/
    public static T OnScrollInvalidated<T>(this T control, Action<System.EventArgs> action) where T : Nodify.NodifyEditor => 
        control._setEvent((System.EventHandler) ((arg0, arg1) => action(arg1)), h => control.ScrollInvalidated += h);


 // ViewportUpdated

/*ActionToEventGenerator*/
    public static T OnViewportUpdated<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : Nodify.NodifyEditor => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.ViewportUpdated += h);



//================= Styles ======================//
 // ViewportZoomProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ViewportZoom<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportZoomProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ViewportZoom<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportZoomProperty, binding);


 // MinViewportZoomProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MinViewportZoom<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.MinViewportZoomProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MinViewportZoom<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.MinViewportZoomProperty, binding);


 // MaxViewportZoomProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxViewportZoom<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.MaxViewportZoomProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxViewportZoom<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.MaxViewportZoomProperty, binding);


 // ViewportSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ViewportSizeProperty, new Avalonia.Size(width, height));public static Style<T> ViewportSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ViewportSizeProperty, new Avalonia.Size(vector2));


 // ItemsExtentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Rect value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Size size) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, new Avalonia.Rect(size));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, new Avalonia.Rect(position, size));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.ItemsExtentProperty, new Avalonia.Rect(topLeft, bottomRight));


 // DecoratorsExtentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DecoratorsExtent<T>(this Style<T> style, Avalonia.Rect value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DecoratorsExtent<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> DecoratorsExtent<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> DecoratorsExtent<T>(this Style<T> style, Avalonia.Size size) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, new Avalonia.Rect(size));public static Style<T> DecoratorsExtent<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, new Avalonia.Rect(position, size));public static Style<T> DecoratorsExtent<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : Nodify.NodifyEditor
   => style._addSetter(Nodify.NodifyEditor.DecoratorsExtentProperty, new Avalonia.Rect(topLeft, bottomRight));


 // ViewportTransformProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ViewportTransform<T>(this Style<T> style, Avalonia.Media.Transform value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportTransformProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ViewportTransform<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ViewportTransformProperty, binding);


 // DpiScaledViewportTransformProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DpiScaledViewportTransform<T>(this Style<T> style, Avalonia.Media.Transform value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DpiScaledViewportTransform<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DpiScaledViewportTransformProperty, binding);


 // BringIntoViewSpeedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> BringIntoViewSpeed<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.BringIntoViewSpeedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BringIntoViewSpeed<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.BringIntoViewSpeedProperty, binding);


 // BringIntoViewMaxDurationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> BringIntoViewMaxDuration<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> BringIntoViewMaxDuration<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.BringIntoViewMaxDurationProperty, binding);


 // DisplayConnectionsOnTopProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisplayConnectionsOnTop<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisplayConnectionsOnTop<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisplayConnectionsOnTopProperty, binding);


 // DisableAutoPanningProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisableAutoPanning<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisableAutoPanningProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisableAutoPanning<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisableAutoPanningProperty, binding);


 // AutoPanSpeedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AutoPanSpeed<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.AutoPanSpeedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AutoPanSpeed<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.AutoPanSpeedProperty, binding);


 // AutoPanEdgeDistanceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> AutoPanEdgeDistance<T>(this Style<T> style, System.Double value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> AutoPanEdgeDistance<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.AutoPanEdgeDistanceProperty, binding);


 // ConnectionTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ConnectionTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ConnectionTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionTemplateProperty, binding);


 // DecoratorTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DecoratorTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DecoratorTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorTemplateProperty, binding);


 // PendingConnectionTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PendingConnectionTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.PendingConnectionTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PendingConnectionTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.PendingConnectionTemplateProperty, binding);


 // SelectionRectangleStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectionRectangleStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectionRectangleStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectionRectangleStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectionRectangleStyleProperty, binding);


 // CuttingLineStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CuttingLineStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingLineStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CuttingLineStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingLineStyleProperty, binding);


 // DecoratorContainerStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DecoratorContainerStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorContainerStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DecoratorContainerStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorContainerStyleProperty, binding);


 // ConnectionsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Connections<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Connections<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionsProperty, binding);


 // SelectedItemsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedItems<T>(this Style<T> style, System.Collections.IList value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedItemsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedItems<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedItemsProperty, binding);


 // SelectedConnectionsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedConnections<T>(this Style<T> style, System.Collections.IList value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedConnectionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedConnections<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedConnectionsProperty, binding);


 // SelectedConnectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedConnection<T>(this Style<T> style, System.Object value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedConnectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedConnection<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.SelectedConnectionProperty, binding);


 // PendingConnectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> PendingConnection<T>(this Style<T> style, System.Object value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.PendingConnectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> PendingConnection<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.PendingConnectionProperty, binding);


 // GridCellSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> GridCellSize<T>(this Style<T> style, System.UInt32 value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.GridCellSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> GridCellSize<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.GridCellSizeProperty, binding);


 // DisableZoomingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisableZooming<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisableZoomingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisableZooming<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisableZoomingProperty, binding);


 // DisablePanningProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisablePanning<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisablePanningProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisablePanning<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisablePanningProperty, binding);


 // EnableRealtimeSelectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EnableRealtimeSelection<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EnableRealtimeSelection<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.EnableRealtimeSelectionProperty, binding);


 // DecoratorsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Decorators<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Decorators<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DecoratorsProperty, binding);


 // CanSelectMultipleConnectionsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanSelectMultipleConnections<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanSelectMultipleConnections<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CanSelectMultipleConnectionsProperty, binding);


 // CanSelectMultipleItemsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CanSelectMultipleItems<T>(this Style<T> style, System.Boolean value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CanSelectMultipleItems<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CanSelectMultipleItemsProperty, binding);


 // ConnectionCompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ConnectionCompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ConnectionCompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionCompletedCommandProperty, binding);


 // ConnectionStartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ConnectionStartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionStartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ConnectionStartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ConnectionStartedCommandProperty, binding);


 // DisconnectConnectorCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisconnectConnectorCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisconnectConnectorCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.DisconnectConnectorCommandProperty, binding);


 // RemoveConnectionCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> RemoveConnectionCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.RemoveConnectionCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> RemoveConnectionCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.RemoveConnectionCommandProperty, binding);


 // ItemsDragStartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsDragStartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsDragStartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsDragStartedCommandProperty, binding);


 // ItemsDragCompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsDragCompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsDragCompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsDragCompletedCommandProperty, binding);


 // ItemsSelectStartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSelectStartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSelectStartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsSelectStartedCommandProperty, binding);


 // ItemsSelectCompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsSelectCompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsSelectCompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.ItemsSelectCompletedCommandProperty, binding);


 // CuttingStartedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CuttingStartedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingStartedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CuttingStartedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingStartedCommandProperty, binding);


 // CuttingCompletedCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> CuttingCompletedCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingCompletedCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> CuttingCompletedCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyEditor
=> style._addSetter(Nodify.NodifyEditor.CuttingCompletedCommandProperty, binding);



}
