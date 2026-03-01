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
public static partial class StepConnection_MarkupExtensions
{
//================= Properties ======================//
 // SourcePositionProperty

/*BindFromExpressionSetterGenerator*/
public static T SourcePosition<T>(this T control, Func<Nodify.ConnectorPosition> func, Action<Nodify.ConnectorPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.SourcePositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SourcePosition<T>(this T control, Nodify.ConnectorPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StepConnection
=> control._setEx(Nodify.StepConnection.SourcePositionProperty, ps, () => control.SourcePosition = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SourcePosition<T>(this T control, IBinding binding) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.SourcePositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SourcePosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.SourcePositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SourcePosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectorPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StepConnection
=> control._setEx(Nodify.StepConnection.SourcePositionProperty, ps, () => control.SourcePosition = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // TargetPositionProperty

/*BindFromExpressionSetterGenerator*/
public static T TargetPosition<T>(this T control, Func<Nodify.ConnectorPosition> func, Action<Nodify.ConnectorPosition>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.TargetPositionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T TargetPosition<T>(this T control, Nodify.ConnectorPosition value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StepConnection
=> control._setEx(Nodify.StepConnection.TargetPositionProperty, ps, () => control.TargetPosition = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T TargetPosition<T>(this T control, IBinding binding) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.TargetPositionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T TargetPosition<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.StepConnection
   => control._set(Nodify.StepConnection.TargetPositionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T TargetPosition<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Nodify.ConnectorPosition> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StepConnection
=> control._setEx(Nodify.StepConnection.TargetPositionProperty, ps, () => control.TargetPosition = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//

}
