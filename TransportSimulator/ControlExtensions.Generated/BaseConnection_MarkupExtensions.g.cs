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
public static partial class BaseConnection_MarkupExtensions
{
//================= Properties ======================//
 // SourceProperty

/*BindFromExpressionSetterGenerator*/
public static T Source<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Source<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceProperty, ps, () => control.Source = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Source<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Source<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Source<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceProperty, ps, () => control.Source = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetProperty

/*BindFromExpressionSetterGenerator*/
public static T Target<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Target<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetProperty, ps, () => control.Target = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Target<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Target<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Target<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetProperty, ps, () => control.Target = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceOffsetProperty

/*BindFromExpressionSetterGenerator*/
public static T SourceOffset<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourceOffset<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOffsetProperty, ps, () => control.SourceOffset = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T SourceOffset<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.BaseConnection
   => control._set(() => control.SourceOffset = new Avalonia.Size(width, height));
public static T SourceOffset<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.BaseConnection
   => control._set(() => control.SourceOffset = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T SourceOffset<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourceOffset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourceOffset<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOffsetProperty, ps, () => control.SourceOffset = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetOffsetProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetOffset<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetOffset<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOffsetProperty, ps, () => control.TargetOffset = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T TargetOffset<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.BaseConnection
   => control._set(() => control.TargetOffset = new Avalonia.Size(width, height));
public static T TargetOffset<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.BaseConnection
   => control._set(() => control.TargetOffset = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T TargetOffset<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetOffset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetOffset<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOffsetProperty, ps, () => control.TargetOffset = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceOffsetModeProperty

/*BindFromExpressionSetterGenerator*/
public static T SourceOffsetMode<T>(this T control, Func<Nodify.ConnectionOffsetMode> func, Action<Nodify.ConnectionOffsetMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourceOffsetMode<T>(this T control, Nodify.ConnectionOffsetMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOffsetModeProperty, ps, () => control.SourceOffsetMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SourceOffsetMode<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourceOffsetMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOffsetModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourceOffsetMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectionOffsetMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOffsetModeProperty, ps, () => control.SourceOffsetMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetOffsetModeProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetOffsetMode<T>(this T control, Func<Nodify.ConnectionOffsetMode> func, Action<Nodify.ConnectionOffsetMode>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetModeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetOffsetMode<T>(this T control, Nodify.ConnectionOffsetMode value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOffsetModeProperty, ps, () => control.TargetOffsetMode = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetOffsetMode<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetModeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetOffsetMode<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOffsetModeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetOffsetMode<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectionOffsetMode> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOffsetModeProperty, ps, () => control.TargetOffsetMode = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SourceOrientationProperty

/*BindFromExpressionSetterGenerator*/
public static T SourceOrientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourceOrientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOrientationProperty, ps, () => control.SourceOrientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SourceOrientation<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourceOrientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SourceOrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourceOrientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SourceOrientationProperty, ps, () => control.SourceOrientation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetOrientationProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetOrientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetOrientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOrientationProperty, ps, () => control.TargetOrientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetOrientation<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetOrientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TargetOrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetOrientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TargetOrientationProperty, ps, () => control.TargetOrientation = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DirectionProperty

/*BindFromExpressionSetterGenerator*/
public static T Direction<T>(this T control, Func<Nodify.ConnectionDirection> func, Action<Nodify.ConnectionDirection>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Direction<T>(this T control, Nodify.ConnectionDirection value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionProperty, ps, () => control.Direction = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Direction<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Direction<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Direction<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectionDirection> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionProperty, ps, () => control.Direction = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DirectionalArrowsCountProperty

/*BindFromExpressionSetterGenerator*/
public static T DirectionalArrowsCount<T>(this T control, Func<System.UInt32> func, Action<System.UInt32>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsCountProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DirectionalArrowsCount<T>(this T control, System.UInt32 value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsCountProperty, ps, () => control.DirectionalArrowsCount = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DirectionalArrowsCount<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsCountProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DirectionalArrowsCount<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsCountProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DirectionalArrowsCount<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.UInt32> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsCountProperty, ps, () => control.DirectionalArrowsCount = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DirectionalArrowsOffsetProperty

/*BindFromExpressionSetterGenerator*/
public static T DirectionalArrowsOffset<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DirectionalArrowsOffset<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, ps, () => control.DirectionalArrowsOffset = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DirectionalArrowsOffset<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DirectionalArrowsOffset<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DirectionalArrowsOffset<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, ps, () => control.DirectionalArrowsOffset = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsAnimatingDirectionalArrowsProperty

/*BindFromExpressionSetterGenerator*/
public static T IsAnimatingDirectionalArrows<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsAnimatingDirectionalArrows<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, ps, () => control.IsAnimatingDirectionalArrows = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsAnimatingDirectionalArrows<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsAnimatingDirectionalArrows<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsAnimatingDirectionalArrows<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, ps, () => control.IsAnimatingDirectionalArrows = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DirectionalArrowsAnimationDurationProperty

/*BindFromExpressionSetterGenerator*/
public static T DirectionalArrowsAnimationDuration<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DirectionalArrowsAnimationDuration<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, ps, () => control.DirectionalArrowsAnimationDuration = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DirectionalArrowsAnimationDuration<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DirectionalArrowsAnimationDuration<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DirectionalArrowsAnimationDuration<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, ps, () => control.DirectionalArrowsAnimationDuration = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SpacingProperty

/*BindFromExpressionSetterGenerator*/
public static T Spacing<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SpacingProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Spacing<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SpacingProperty, ps, () => control.Spacing = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Spacing<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SpacingProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Spacing<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SpacingProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Spacing<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SpacingProperty, ps, () => control.Spacing = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ArrowSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ArrowSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ArrowSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowSizeProperty, ps, () => control.ArrowSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ArrowSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.BaseConnection
   => control._set(() => control.ArrowSize = new Avalonia.Size(width, height));
public static T ArrowSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.BaseConnection
   => control._set(() => control.ArrowSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ArrowSize<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ArrowSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ArrowSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowSizeProperty, ps, () => control.ArrowSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ArrowEndsProperty

/*BindFromExpressionSetterGenerator*/
public static T ArrowEnds<T>(this T control, Func<Nodify.ArrowHeadEnds> func, Action<Nodify.ArrowHeadEnds>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowEndsProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ArrowEnds<T>(this T control, Nodify.ArrowHeadEnds value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowEndsProperty, ps, () => control.ArrowEnds = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ArrowEnds<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowEndsProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ArrowEnds<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowEndsProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ArrowEnds<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ArrowHeadEnds> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowEndsProperty, ps, () => control.ArrowEnds = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ArrowShapeProperty

/*BindFromExpressionSetterGenerator*/
public static T ArrowShape<T>(this T control, Func<Nodify.ArrowHeadShape> func, Action<Nodify.ArrowHeadShape>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowShapeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ArrowShape<T>(this T control, Nodify.ArrowHeadShape value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowShapeProperty, ps, () => control.ArrowShape = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ArrowShape<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowShapeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ArrowShape<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ArrowShapeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ArrowShape<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ArrowHeadShape> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ArrowShapeProperty, ps, () => control.ArrowShape = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SplitCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T SplitCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SplitCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SplitCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SplitCommandProperty, ps, () => control.SplitCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SplitCommand<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SplitCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SplitCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.SplitCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SplitCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.SplitCommandProperty, ps, () => control.SplitCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DisconnectCommandProperty

/*BindFromExpressionSetterGenerator*/
public static T DisconnectCommand<T>(this T control, Func<System.Windows.Input.ICommand> func, Action<System.Windows.Input.ICommand>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DisconnectCommandProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DisconnectCommand<T>(this T control, System.Windows.Input.ICommand value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DisconnectCommandProperty, ps, () => control.DisconnectCommand = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DisconnectCommand<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DisconnectCommandProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DisconnectCommand<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.DisconnectCommandProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DisconnectCommand<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Windows.Input.ICommand> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.DisconnectCommandProperty, ps, () => control.DisconnectCommand = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OutlineThicknessProperty

/*BindFromExpressionSetterGenerator*/
public static T OutlineThickness<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OutlineThickness<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.OutlineThicknessProperty, ps, () => control.OutlineThickness = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OutlineThickness<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OutlineThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OutlineThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.OutlineThicknessProperty, ps, () => control.OutlineThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OutlineBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T OutlineBrush<T>(this T control, Func<Avalonia.Media.IBrush?> func, Action<Avalonia.Media.IBrush?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OutlineBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.OutlineBrushProperty, ps, () => control.OutlineBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OutlineBrush<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OutlineBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.OutlineBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OutlineBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.OutlineBrushProperty, ps, () => control.OutlineBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ForegroundProperty

/*BindFromExpressionSetterGenerator*/
public static T Foreground<T>(this T control, Func<Avalonia.Media.IBrush?> func, Action<Avalonia.Media.IBrush?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ForegroundProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Foreground<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ForegroundProperty, ps, () => control.Foreground = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Foreground<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ForegroundProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Foreground<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.ForegroundProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Foreground<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.ForegroundProperty, ps, () => control.Foreground = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TextProperty

/*BindFromExpressionSetterGenerator*/
public static T Text<T>(this T control, Func<System.String?> func, Action<System.String?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TextProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Text<T>(this T control, System.String value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TextProperty, ps, () => control.Text = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Text<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TextProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Text<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.TextProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Text<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.String> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.TextProperty, ps, () => control.Text = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T FontSize<T>(this T control, Func<System.Double> func, Action<System.Double>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontSize<T>(this T control, System.Double value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontSizeProperty, ps, () => control.FontSize = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontSize<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Double> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontSizeProperty, ps, () => control.FontSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontFamilyProperty

/*BindFromExpressionSetterGenerator*/
public static T FontFamily<T>(this T control, Func<Avalonia.Media.FontFamily> func, Action<Avalonia.Media.FontFamily>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontFamilyProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontFamily<T>(this T control, Avalonia.Media.FontFamily value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontFamilyProperty, ps, () => control.FontFamily = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontFamily<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontFamilyProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontFamily<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontFamilyProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontFamily<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontFamily> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontFamilyProperty, ps, () => control.FontFamily = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontWeightProperty

/*BindFromExpressionSetterGenerator*/
public static T FontWeight<T>(this T control, Func<Avalonia.Media.FontWeight> func, Action<Avalonia.Media.FontWeight>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontWeightProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontWeight<T>(this T control, Avalonia.Media.FontWeight value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontWeightProperty, ps, () => control.FontWeight = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontWeight<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontWeightProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontWeight<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontWeightProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontWeight<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontWeight> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontWeightProperty, ps, () => control.FontWeight = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T FontStyle<T>(this T control, Func<Avalonia.Media.FontStyle> func, Action<Avalonia.Media.FontStyle>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontStyle<T>(this T control, Avalonia.Media.FontStyle value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontStyleProperty, ps, () => control.FontStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontStyle<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStyle> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontStyleProperty, ps, () => control.FontStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FontStretchProperty

/*BindFromExpressionSetterGenerator*/
public static T FontStretch<T>(this T control, Func<Avalonia.Media.FontStretch> func, Action<Avalonia.Media.FontStretch>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStretchProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FontStretch<T>(this T control, Avalonia.Media.FontStretch value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontStretchProperty, ps, () => control.FontStretch = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FontStretch<T>(this T control, IBinding binding) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStretchProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FontStretch<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.BaseConnection
   => control._set(Nodify.BaseConnection.FontStretchProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FontStretch<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.FontStretch> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.BaseConnection
=> control._setEx(Nodify.BaseConnection.FontStretchProperty, ps, () => control.FontStretch = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // Disconnect

/*ActionToEventGenerator*/
    public static T OnDisconnect<T>(this T control, Action<Nodify.ConnectionEventArgs> action) where T : Nodify.BaseConnection => 
        control._setEvent((Nodify.ConnectionEventHandler) ((arg0, arg1) => action(arg1)), h => control.Disconnect += h);


 // Split

/*ActionToEventGenerator*/
    public static T OnSplit<T>(this T control, Action<Nodify.ConnectionEventArgs> action) where T : Nodify.BaseConnection => 
        control._setEvent((Nodify.ConnectionEventHandler) ((arg0, arg1) => action(arg1)), h => control.Split += h);



//================= Styles ======================//
 // SourceProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Source<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceProperty, binding);


 // TargetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Target<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetProperty, binding);


 // SourceOffsetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SourceOffset<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SourceOffset<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOffsetProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> SourceOffset<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.SourceOffsetProperty, new Avalonia.Size(width, height));public static Style<T> SourceOffset<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.SourceOffsetProperty, new Avalonia.Size(vector2));


 // TargetOffsetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TargetOffset<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetOffset<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOffsetProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> TargetOffset<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.TargetOffsetProperty, new Avalonia.Size(width, height));public static Style<T> TargetOffset<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.TargetOffsetProperty, new Avalonia.Size(vector2));


 // SourceOffsetModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SourceOffsetMode<T>(this Style<T> style, Nodify.ConnectionOffsetMode value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOffsetModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SourceOffsetMode<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOffsetModeProperty, binding);


 // TargetOffsetModeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TargetOffsetMode<T>(this Style<T> style, Nodify.ConnectionOffsetMode value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOffsetModeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetOffsetMode<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOffsetModeProperty, binding);


 // SourceOrientationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SourceOrientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOrientationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SourceOrientation<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SourceOrientationProperty, binding);


 // TargetOrientationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> TargetOrientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOrientationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> TargetOrientation<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TargetOrientationProperty, binding);


 // DirectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Direction<T>(this Style<T> style, Nodify.ConnectionDirection value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Direction<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionProperty, binding);


 // DirectionalArrowsCountProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DirectionalArrowsCount<T>(this Style<T> style, System.UInt32 value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsCountProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DirectionalArrowsCount<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsCountProperty, binding);


 // DirectionalArrowsOffsetProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DirectionalArrowsOffset<T>(this Style<T> style, System.Double value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DirectionalArrowsOffset<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsOffsetProperty, binding);


 // IsAnimatingDirectionalArrowsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsAnimatingDirectionalArrows<T>(this Style<T> style, System.Boolean value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsAnimatingDirectionalArrows<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.IsAnimatingDirectionalArrowsProperty, binding);


 // DirectionalArrowsAnimationDurationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DirectionalArrowsAnimationDuration<T>(this Style<T> style, System.Double value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DirectionalArrowsAnimationDuration<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DirectionalArrowsAnimationDurationProperty, binding);


 // SpacingProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Spacing<T>(this Style<T> style, System.Double value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SpacingProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Spacing<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SpacingProperty, binding);


 // ArrowSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ArrowSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ArrowSize<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ArrowSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.ArrowSizeProperty, new Avalonia.Size(width, height));public static Style<T> ArrowSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.BaseConnection
   => style._addSetter(Nodify.BaseConnection.ArrowSizeProperty, new Avalonia.Size(vector2));


 // ArrowEndsProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ArrowEnds<T>(this Style<T> style, Nodify.ArrowHeadEnds value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowEndsProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ArrowEnds<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowEndsProperty, binding);


 // ArrowShapeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ArrowShape<T>(this Style<T> style, Nodify.ArrowHeadShape value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowShapeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ArrowShape<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ArrowShapeProperty, binding);


 // SplitCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SplitCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SplitCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SplitCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.SplitCommandProperty, binding);


 // DisconnectCommandProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DisconnectCommand<T>(this Style<T> style, System.Windows.Input.ICommand value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DisconnectCommandProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DisconnectCommand<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.DisconnectCommandProperty, binding);


 // OutlineThicknessProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OutlineThickness<T>(this Style<T> style, System.Double value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.OutlineThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OutlineThickness<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.OutlineThicknessProperty, binding);


 // OutlineBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OutlineBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.OutlineBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OutlineBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.OutlineBrushProperty, binding);


 // ForegroundProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Foreground<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ForegroundProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Foreground<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.ForegroundProperty, binding);


 // TextProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, System.String value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TextProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Text<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.TextProperty, binding);


 // FontSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, System.Double value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontSize<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontSizeProperty, binding);


 // FontFamilyProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontFamily<T>(this Style<T> style, Avalonia.Media.FontFamily value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontFamilyProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontFamily<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontFamilyProperty, binding);


 // FontWeightProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontWeight<T>(this Style<T> style, Avalonia.Media.FontWeight value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontWeightProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontWeight<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontWeightProperty, binding);


 // FontStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontStyle<T>(this Style<T> style, Avalonia.Media.FontStyle value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontStyleProperty, binding);


 // FontStretchProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FontStretch<T>(this Style<T> style, Avalonia.Media.FontStretch value) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontStretchProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FontStretch<T>(this Style<T> style, IBinding binding) where T : Nodify.BaseConnection
=> style._addSetter(Nodify.BaseConnection.FontStretchProperty, binding);



}
