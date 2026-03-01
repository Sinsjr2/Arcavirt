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
public static partial class Minimap_MarkupExtensions
{
//================= Properties ======================//
 // ViewportLocationProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportLocation<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportLocationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportLocation<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportLocationProperty, ps, () => control.ViewportLocation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ViewportLocation<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportLocationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportLocation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportLocationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportLocation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportLocationProperty, ps, () => control.ViewportLocation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ViewportSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportSizeProperty, ps, () => control.ViewportSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ViewportSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.Minimap
   => control._set(() => control.ViewportSize = new Avalonia.Size(width, height));
public static T ViewportSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.Minimap
   => control._set(() => control.ViewportSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ViewportSize<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportSizeProperty, ps, () => control.ViewportSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ViewportStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T ViewportStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ViewportStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportStyleProperty, ps, () => control.ViewportStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ViewportStyle<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ViewportStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ViewportStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ViewportStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ViewportStyleProperty, ps, () => control.ViewportStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ExtentProperty

/*BindFromExpressionSetterGenerator*/
public static T Extent<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ExtentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Extent<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ExtentProperty, ps, () => control.Extent = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Extent<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : Nodify.Minimap
   => control._set(() => control.Extent = new Avalonia.Rect(x, y, width, height));
public static T Extent<T>(this T control, Avalonia.Size size = default) where T : Nodify.Minimap
   => control._set(() => control.Extent = new Avalonia.Rect(size));
public static T Extent<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : Nodify.Minimap
   => control._set(() => control.Extent = new Avalonia.Rect(position, size));
public static T Extent<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : Nodify.Minimap
   => control._set(() => control.Extent = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T Extent<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ExtentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Extent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ExtentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Extent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ExtentProperty, ps, () => control.Extent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ItemsExtentProperty

/*BindFromExpressionSetterGenerator*/
public static T ItemsExtent<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ItemsExtentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ItemsExtent<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ItemsExtentProperty, ps, () => control.ItemsExtent = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ItemsExtent<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : Nodify.Minimap
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(x, y, width, height));
public static T ItemsExtent<T>(this T control, Avalonia.Size size = default) where T : Nodify.Minimap
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(size));
public static T ItemsExtent<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : Nodify.Minimap
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(position, size));
public static T ItemsExtent<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : Nodify.Minimap
   => control._set(() => control.ItemsExtent = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T ItemsExtent<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ItemsExtentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ItemsExtent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ItemsExtentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ItemsExtent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ItemsExtentProperty, ps, () => control.ItemsExtent = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // MaxViewportOffsetProperty

/*BindFromExpressionSetterGenerator*/
public static T MaxViewportOffset<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.MaxViewportOffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T MaxViewportOffset<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.MaxViewportOffsetProperty, ps, () => control.MaxViewportOffset = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T MaxViewportOffset<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.Minimap
   => control._set(() => control.MaxViewportOffset = new Avalonia.Size(width, height));
public static T MaxViewportOffset<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.Minimap
   => control._set(() => control.MaxViewportOffset = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T MaxViewportOffset<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.MaxViewportOffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T MaxViewportOffset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.MaxViewportOffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T MaxViewportOffset<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.MaxViewportOffsetProperty, ps, () => control.MaxViewportOffset = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ResizeToViewportProperty

/*BindFromExpressionSetterGenerator*/
public static T ResizeToViewport<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ResizeToViewportProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ResizeToViewport<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ResizeToViewportProperty, ps, () => control.ResizeToViewport = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ResizeToViewport<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ResizeToViewportProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ResizeToViewport<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.ResizeToViewportProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ResizeToViewport<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.ResizeToViewportProperty, ps, () => control.ResizeToViewport = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsReadOnlyProperty

/*BindFromExpressionSetterGenerator*/
public static T IsReadOnly<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.IsReadOnlyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsReadOnly<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.IsReadOnlyProperty, ps, () => control.IsReadOnly = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsReadOnly<T>(this T control, IBinding binding) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.IsReadOnlyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsReadOnly<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Minimap
   => control._set(Nodify.Minimap.IsReadOnlyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsReadOnly<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Minimap
=> control._setEx(Nodify.Minimap.IsReadOnlyProperty, ps, () => control.IsReadOnly = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Zoom

/*ActionToEventGenerator*/
    public static T OnZoom<T>(this T control, Action<Nodify.ZoomEventArgs> action) where T : Nodify.Minimap => 
        control._setEvent((Nodify.ZoomEventHandler) ((arg0, arg1) => action(arg1)), h => control.Zoom += h);



//================= Styles ======================//
 // ViewportSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ViewportSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ViewportSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ViewportSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ViewportSizeProperty, new Avalonia.Size(width, height));public static Style<T> ViewportSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ViewportSizeProperty, new Avalonia.Size(vector2));


 // ViewportStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ViewportStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ViewportStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ViewportStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ViewportStyleProperty, binding);


 // ExtentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, Avalonia.Rect value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ExtentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ExtentProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ExtentProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Size size) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ExtentProperty, new Avalonia.Rect(size));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ExtentProperty, new Avalonia.Rect(position, size));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ExtentProperty, new Avalonia.Rect(topLeft, bottomRight));


 // ItemsExtentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Rect value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ItemsExtentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ItemsExtentProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ItemsExtent<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ItemsExtentProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Size size) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ItemsExtentProperty, new Avalonia.Rect(size));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ItemsExtentProperty, new Avalonia.Rect(position, size));public static Style<T> ItemsExtent<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.ItemsExtentProperty, new Avalonia.Rect(topLeft, bottomRight));


 // MaxViewportOffsetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> MaxViewportOffset<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.MaxViewportOffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> MaxViewportOffset<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.MaxViewportOffsetProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> MaxViewportOffset<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.MaxViewportOffsetProperty, new Avalonia.Size(width, height));public static Style<T> MaxViewportOffset<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.Minimap
   => style._addSetter(Nodify.Minimap.MaxViewportOffsetProperty, new Avalonia.Size(vector2));


 // ResizeToViewportProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ResizeToViewport<T>(this Style<T> style, System.Boolean value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ResizeToViewportProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ResizeToViewport<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.ResizeToViewportProperty, binding);


 // IsReadOnlyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, System.Boolean value) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.IsReadOnlyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsReadOnly<T>(this Style<T> style, IBinding binding) where T : Nodify.Minimap
=> style._addSetter(Nodify.Minimap.IsReadOnlyProperty, binding);



}
