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
public static partial class NodifyCanvas_MarkupExtensions
{
//================= Properties ======================//
 // ExtentProperty

/*BindFromExpressionSetterGenerator*/
public static T Extent<T>(this T control, Func<Avalonia.Rect> func, Action<Avalonia.Rect>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodifyCanvas
   => control._set(Nodify.NodifyCanvas.ExtentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Extent<T>(this T control, Avalonia.Rect value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyCanvas
=> control._setEx(Nodify.NodifyCanvas.ExtentProperty, ps, () => control.Extent = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T Extent<T>(this T control, System.Double x = default, System.Double y = default, System.Double width = default, System.Double height = default) where T : Nodify.NodifyCanvas
   => control._set(() => control.Extent = new Avalonia.Rect(x, y, width, height));
public static T Extent<T>(this T control, Avalonia.Size size = default) where T : Nodify.NodifyCanvas
   => control._set(() => control.Extent = new Avalonia.Rect(size));
public static T Extent<T>(this T control, Avalonia.Point position = default, Avalonia.Size size = default) where T : Nodify.NodifyCanvas
   => control._set(() => control.Extent = new Avalonia.Rect(position, size));
public static T Extent<T>(this T control, Avalonia.Point topLeft = default, Avalonia.Point bottomRight = default) where T : Nodify.NodifyCanvas
   => control._set(() => control.Extent = new Avalonia.Rect(topLeft, bottomRight));

/*BindSetterGenerator*/
public static T Extent<T>(this T control, IBinding binding) where T : Nodify.NodifyCanvas
   => control._set(Nodify.NodifyCanvas.ExtentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Extent<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodifyCanvas
   => control._set(Nodify.NodifyCanvas.ExtentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Extent<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Rect> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodifyCanvas
=> control._setEx(Nodify.NodifyCanvas.ExtentProperty, ps, () => control.Extent = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ExtentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, Avalonia.Rect value) where T : Nodify.NodifyCanvas
=> style._addSetter(Nodify.NodifyCanvas.ExtentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, IBinding binding) where T : Nodify.NodifyCanvas
=> style._addSetter(Nodify.NodifyCanvas.ExtentProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> Extent<T>(this Style<T> style, System.Double x, System.Double y, System.Double width, System.Double height) where T : Nodify.NodifyCanvas
   => style._addSetter(Nodify.NodifyCanvas.ExtentProperty, new Avalonia.Rect(x, y, width, height));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Size size) where T : Nodify.NodifyCanvas
   => style._addSetter(Nodify.NodifyCanvas.ExtentProperty, new Avalonia.Rect(size));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Point position, Avalonia.Size size) where T : Nodify.NodifyCanvas
   => style._addSetter(Nodify.NodifyCanvas.ExtentProperty, new Avalonia.Rect(position, size));public static Style<T> Extent<T>(this Style<T> style, Avalonia.Point topLeft, Avalonia.Point bottomRight) where T : Nodify.NodifyCanvas
   => style._addSetter(Nodify.NodifyCanvas.ExtentProperty, new Avalonia.Rect(topLeft, bottomRight));



}
