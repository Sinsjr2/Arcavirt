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
public static partial class CuttingLine_MarkupExtensions
{
//================= Properties ======================//
 // StartPointProperty

/*BindFromExpressionSetterGenerator*/
public static T StartPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.StartPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T StartPoint<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CuttingLine
=> control._setEx(Nodify.CuttingLine.StartPointProperty, ps, () => control.StartPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T StartPoint<T>(this T control, IBinding binding) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.StartPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T StartPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.StartPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T StartPoint<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CuttingLine
=> control._setEx(Nodify.CuttingLine.StartPointProperty, ps, () => control.StartPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // EndPointProperty

/*BindFromExpressionSetterGenerator*/
public static T EndPoint<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.EndPointProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T EndPoint<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CuttingLine
=> control._setEx(Nodify.CuttingLine.EndPointProperty, ps, () => control.EndPoint = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T EndPoint<T>(this T control, IBinding binding) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.EndPointProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T EndPoint<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.CuttingLine
   => control._set(Nodify.CuttingLine.EndPointProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T EndPoint<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CuttingLine
=> control._setEx(Nodify.CuttingLine.EndPointProperty, ps, () => control.EndPoint = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // StartPointProperty

/*ValueStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.CuttingLine
=> style._addSetter(Nodify.CuttingLine.StartPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> StartPoint<T>(this Style<T> style, IBinding binding) where T : Nodify.CuttingLine
=> style._addSetter(Nodify.CuttingLine.StartPointProperty, binding);


 // EndPointProperty

/*ValueStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.CuttingLine
=> style._addSetter(Nodify.CuttingLine.EndPointProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> EndPoint<T>(this Style<T> style, IBinding binding) where T : Nodify.CuttingLine
=> style._addSetter(Nodify.CuttingLine.EndPointProperty, binding);



}
