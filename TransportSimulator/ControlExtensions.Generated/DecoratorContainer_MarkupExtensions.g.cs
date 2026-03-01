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
public static partial class DecoratorContainer_MarkupExtensions
{
//================= Properties ======================//
 // LocationProperty

/*BindFromExpressionSetterGenerator*/
public static T Location<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.LocationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Location<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.DecoratorContainer
=> control._setEx(Nodify.DecoratorContainer.LocationProperty, ps, () => control.Location = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Location<T>(this T control, IBinding binding) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.LocationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Location<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.LocationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Location<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.DecoratorContainer
=> control._setEx(Nodify.DecoratorContainer.LocationProperty, ps, () => control.Location = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActualSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ActualSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.ActualSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActualSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.DecoratorContainer
=> control._setEx(Nodify.DecoratorContainer.ActualSizeProperty, ps, () => control.ActualSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ActualSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.DecoratorContainer
   => control._set(() => control.ActualSize = new Avalonia.Size(width, height));
public static T ActualSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.DecoratorContainer
   => control._set(() => control.ActualSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ActualSize<T>(this T control, IBinding binding) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.ActualSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActualSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.DecoratorContainer
   => control._set(Nodify.DecoratorContainer.ActualSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActualSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.DecoratorContainer
=> control._setEx(Nodify.DecoratorContainer.ActualSizeProperty, ps, () => control.ActualSize = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // LocationChanged

/*ActionToEventGenerator*/
    public static T OnLocationChanged<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : Nodify.DecoratorContainer => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LocationChanged += h);



//================= Styles ======================//
 // LocationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.DecoratorContainer
=> style._addSetter(Nodify.DecoratorContainer.LocationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, IBinding binding) where T : Nodify.DecoratorContainer
=> style._addSetter(Nodify.DecoratorContainer.LocationProperty, binding);


 // ActualSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.DecoratorContainer
=> style._addSetter(Nodify.DecoratorContainer.ActualSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, IBinding binding) where T : Nodify.DecoratorContainer
=> style._addSetter(Nodify.DecoratorContainer.ActualSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.DecoratorContainer
   => style._addSetter(Nodify.DecoratorContainer.ActualSizeProperty, new Avalonia.Size(width, height));public static Style<T> ActualSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.DecoratorContainer
   => style._addSetter(Nodify.DecoratorContainer.ActualSizeProperty, new Avalonia.Size(vector2));



}
