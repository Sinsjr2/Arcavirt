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
public static partial class MinimapItem_MarkupExtensions
{
//================= Properties ======================//
 // LocationProperty

/*BindFromExpressionSetterGenerator*/
public static T Location<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.MinimapItem
   => control._set(Nodify.MinimapItem.LocationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Location<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.MinimapItem
=> control._setEx(Nodify.MinimapItem.LocationProperty, ps, () => control.Location = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Location<T>(this T control, IBinding binding) where T : Nodify.MinimapItem
   => control._set(Nodify.MinimapItem.LocationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Location<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.MinimapItem
   => control._set(Nodify.MinimapItem.LocationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Location<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.MinimapItem
=> control._setEx(Nodify.MinimapItem.LocationProperty, ps, () => control.Location = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // LocationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.MinimapItem
=> style._addSetter(Nodify.MinimapItem.LocationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, IBinding binding) where T : Nodify.MinimapItem
=> style._addSetter(Nodify.MinimapItem.LocationProperty, binding);



}
