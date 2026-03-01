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
public static partial class Node_MarkupExtensions
{
//================= Properties ======================//
 // ContentBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T ContentBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.ContentBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ContentBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.ContentBrushProperty, ps, () => control.ContentBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentBrush<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.ContentBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.ContentBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ContentBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.ContentBrushProperty, ps, () => control.ContentBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.HeaderBrushProperty, ps, () => control.HeaderBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderBrush<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.HeaderBrushProperty, ps, () => control.HeaderBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterBrushProperty, ps, () => control.FooterBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterBrush<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.FooterBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterBrushProperty, ps, () => control.FooterBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterProperty

/*BindFromExpressionSetterGenerator*/
public static T Footer<T>(this T control, Func<System.Object> func, Action<System.Object>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Footer<T>(this T control, System.Object value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterProperty, ps, () => control.Footer = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Footer<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.FooterProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Footer<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Footer<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Object> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterProperty, ps, () => control.Footer = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterTemplateProperty, ps, () => control.FooterTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterTemplate<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.FooterTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterTemplateProperty, ps, () => control.FooterTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputConnectorTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T InputConnectorTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.InputConnectorTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T InputConnectorTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.InputConnectorTemplateProperty, ps, () => control.InputConnectorTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T InputConnectorTemplate<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.InputConnectorTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T InputConnectorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.InputConnectorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T InputConnectorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.InputConnectorTemplateProperty, ps, () => control.InputConnectorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OutputConnectorTemplateProperty

/*BindFromExpressionSetterGenerator*/
public static T OutputConnectorTemplate<T>(this T control, Func<Avalonia.Markup.Xaml.Templates.DataTemplate> func, Action<Avalonia.Markup.Xaml.Templates.DataTemplate>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.OutputConnectorTemplateProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T OutputConnectorTemplate<T>(this T control, Avalonia.Markup.Xaml.Templates.DataTemplate value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.OutputConnectorTemplateProperty, ps, () => control.OutputConnectorTemplate = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T OutputConnectorTemplate<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.OutputConnectorTemplateProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T OutputConnectorTemplate<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.OutputConnectorTemplateProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T OutputConnectorTemplate<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Markup.Xaml.Templates.DataTemplate> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.OutputConnectorTemplateProperty, ps, () => control.OutputConnectorTemplate = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // InputProperty

/*BindFromExpressionSetterGenerator*/
public static T Input<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.InputProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Input<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.InputProperty, ps, () => control.Input = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Input<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.InputProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Input<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.InputProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Input<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.InputProperty, ps, () => control.Input = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // OutputProperty

/*BindFromExpressionSetterGenerator*/
public static T Output<T>(this T control, Func<System.Collections.IEnumerable> func, Action<System.Collections.IEnumerable>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.OutputProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Output<T>(this T control, System.Collections.IEnumerable value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.OutputProperty, ps, () => control.Output = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Output<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.OutputProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Output<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.OutputProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Output<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Collections.IEnumerable> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.OutputProperty, ps, () => control.Output = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ContentContainerStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T ContentContainerStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.ContentContainerStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ContentContainerStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.ContentContainerStyleProperty, ps, () => control.ContentContainerStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T ContentContainerStyle<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.ContentContainerStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ContentContainerStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.ContentContainerStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ContentContainerStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.ContentContainerStyleProperty, ps, () => control.ContentContainerStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // HeaderContainerStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T HeaderContainerStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderContainerStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HeaderContainerStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.HeaderContainerStyleProperty, ps, () => control.HeaderContainerStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HeaderContainerStyle<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderContainerStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HeaderContainerStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.HeaderContainerStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HeaderContainerStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.HeaderContainerStyleProperty, ps, () => control.HeaderContainerStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // FooterContainerStyleProperty

/*BindFromExpressionSetterGenerator*/
public static T FooterContainerStyle<T>(this T control, Func<Avalonia.Styling.ControlTheme> func, Action<Avalonia.Styling.ControlTheme>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterContainerStyleProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T FooterContainerStyle<T>(this T control, Avalonia.Styling.ControlTheme value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterContainerStyleProperty, ps, () => control.FooterContainerStyle = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T FooterContainerStyle<T>(this T control, IBinding binding) where T : Nodify.Node
   => control._set(Nodify.Node.FooterContainerStyleProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T FooterContainerStyle<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.Node
   => control._set(Nodify.Node.FooterContainerStyleProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T FooterContainerStyle<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Styling.ControlTheme> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.Node
=> control._setEx(Nodify.Node.FooterContainerStyleProperty, ps, () => control.FooterContainerStyle = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//

//================= Styles ======================//
 // ContentBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ContentBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.ContentBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ContentBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.ContentBrushProperty, binding);


 // HeaderBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.HeaderBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.HeaderBrushProperty, binding);


 // FooterBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterBrushProperty, binding);


 // FooterProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, System.Object value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Footer<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterProperty, binding);


 // FooterTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterTemplateProperty, binding);


 // InputConnectorTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> InputConnectorTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.InputConnectorTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> InputConnectorTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.InputConnectorTemplateProperty, binding);


 // OutputConnectorTemplateProperty

/*ValueStyleSetterGenerator*/
public static Style<T> OutputConnectorTemplate<T>(this Style<T> style, Avalonia.Markup.Xaml.Templates.DataTemplate value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.OutputConnectorTemplateProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> OutputConnectorTemplate<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.OutputConnectorTemplateProperty, binding);


 // InputProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Input<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.InputProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Input<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.InputProperty, binding);


 // OutputProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Output<T>(this Style<T> style, System.Collections.IEnumerable value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.OutputProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Output<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.OutputProperty, binding);


 // ContentContainerStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ContentContainerStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.ContentContainerStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ContentContainerStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.ContentContainerStyleProperty, binding);


 // HeaderContainerStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HeaderContainerStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.HeaderContainerStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HeaderContainerStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.HeaderContainerStyleProperty, binding);


 // FooterContainerStyleProperty

/*ValueStyleSetterGenerator*/
public static Style<T> FooterContainerStyle<T>(this Style<T> style, Avalonia.Styling.ControlTheme value) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterContainerStyleProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> FooterContainerStyle<T>(this Style<T> style, IBinding binding) where T : Nodify.Node
=> style._addSetter(Nodify.Node.FooterContainerStyleProperty, binding);



}
