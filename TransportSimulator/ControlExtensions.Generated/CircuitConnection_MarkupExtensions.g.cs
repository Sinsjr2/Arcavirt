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
public static partial class CircuitConnection_MarkupExtensions
{
//================= Properties ======================//
 // AngleProperty

/*BindFromExpressionSetterGenerator*/
public static T Angle<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.CircuitConnection
   => control._set(Nodify.CircuitConnection.AngleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Angle<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CircuitConnection
=> control._setEx(Nodify.CircuitConnection.AngleProperty, ps, () => control.Angle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Angle<T>(this T control, IBinding binding) where T : Nodify.CircuitConnection
   => control._set(Nodify.CircuitConnection.AngleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Angle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.CircuitConnection
   => control._set(Nodify.CircuitConnection.AngleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Angle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.CircuitConnection
=> control._setEx(Nodify.CircuitConnection.AngleProperty, ps, () => control.Angle = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // AngleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Angle<T>(this Style<T> style, System.Double value) where T : Nodify.CircuitConnection
=> style._addSetter(Nodify.CircuitConnection.AngleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Angle<T>(this Style<T> style, IBinding binding) where T : Nodify.CircuitConnection
=> style._addSetter(Nodify.CircuitConnection.AngleProperty, binding);



}
