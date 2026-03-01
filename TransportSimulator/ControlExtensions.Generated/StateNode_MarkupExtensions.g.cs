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
public static partial class StateNode_MarkupExtensions
{
//================= Properties ======================//
 // HighlightBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HighlightBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.HighlightBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HighlightBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.HighlightBrushProperty, ps, () => control.HighlightBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HighlightBrush<T>(this T control, IBinding binding) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.HighlightBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HighlightBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.HighlightBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HighlightBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.HighlightBrushProperty, ps, () => control.HighlightBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ContentProperty

/*BindFromExpressionSetterGenerator*/
public static T Content<T>(this T control, Func<System.Object?> func, Action<System.Object?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Content<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.ContentProperty, ps, () => control.Content = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Content<T>(this T control, IBinding binding) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Content<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Content<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.ContentProperty, ps, () => control.Content = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ContentTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T ContentTemplate<T>(this T control, Func<Avalonia.Controls.Templates.IDataTemplate?> func, Action<Avalonia.Controls.Templates.IDataTemplate?>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ContentTemplate<T>(this T control, Avalonia.Controls.Templates.IDataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.ContentTemplateProperty, ps, () => control.ContentTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentTemplate<T>(this T control, IBinding binding) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.StateNode
   => control._set(Nodify.StateNode.ContentTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ContentTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Controls.Templates.IDataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.StateNode
=> control._setEx(Nodify.StateNode.ContentTemplateProperty, ps, () => control.ContentTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // HighlightBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HighlightBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.HighlightBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HighlightBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.HighlightBrushProperty, binding);


 // ContentProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Content<T>(this Style<T> style, System.Object value) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.ContentProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Content<T>(this Style<T> style, IBinding binding) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.ContentProperty, binding);


 // ContentTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ContentTemplate<T>(this Style<T> style, Avalonia.Controls.Templates.IDataTemplate value) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.ContentTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ContentTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.StateNode
=> style._addSetter(Nodify.StateNode.ContentTemplateProperty, binding);



}
