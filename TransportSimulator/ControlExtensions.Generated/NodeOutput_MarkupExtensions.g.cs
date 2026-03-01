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
public static partial class NodeOutput_MarkupExtensions
{
//================= Properties ======================//
 // HeaderProperty

/*BindFromExpressionSetterGenerator*/
public static T Header<T>(this T control, Func<System.Object?> func, Action<System.Object?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Header<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.HeaderProperty, ps, () => control.Header = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Header<T>(this T control, IBinding binding) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Header<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Header<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.HeaderProperty, ps, () => control.Header = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate?> func, Action<Avalonia.Controls.Templates.IDataTemplate?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.HeaderTemplateProperty, ps, () => control.HeaderTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, IBinding binding) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.HeaderTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.HeaderTemplateProperty, ps, () => control.HeaderTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ConnectorTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T ConnectorTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.ControlTemplate> func, Action<Avalonia.Markup.Xaml.Templates.ControlTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.ConnectorTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ConnectorTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.ControlTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.ConnectorTemplateProperty, ps, () => control.ConnectorTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ConnectorTemplate<T>(this T control, IBinding binding) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.ConnectorTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ConnectorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.ConnectorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ConnectorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.ControlTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.ConnectorTemplateProperty, ps, () => control.ConnectorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OrientationProperty

/*BindFromExpressionSetterGenerator*/
public static T Orientation<T>(this T control, Func<Avalonia.Layout.Orientation> func, Action<Avalonia.Layout.Orientation>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.OrientationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Orientation<T>(this T control, Avalonia.Layout.Orientation value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.OrientationProperty, ps, () => control.Orientation = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Orientation<T>(this T control, IBinding binding) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.OrientationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Orientation<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.NodeOutput
   => control._set(Nodify.NodeOutput.OrientationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Orientation<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Layout.Orientation> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.NodeOutput
=> control._setEx(Nodify.NodeOutput.OrientationProperty, ps, () => control.Orientation = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // HeaderProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, System.Object value) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.HeaderProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Header<T>(this Style<T> style, IBinding binding) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.HeaderProperty, binding);


 // HeaderTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.HeaderTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.HeaderTemplateProperty, binding);


 // ConnectorTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ConnectorTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.ControlTemplate value) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.ConnectorTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ConnectorTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.ConnectorTemplateProperty, binding);


 // OrientationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, Avalonia.Layout.Orientation value) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.OrientationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Orientation<T>(this Style<T> style, IBinding binding) where T : Nodify.NodeOutput
=> style._addSetter(Nodify.NodeOutput.OrientationProperty, binding);



}
