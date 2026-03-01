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
public static partial class ItemContainer_MarkupExtensions
{
//================= Properties ======================//
 // HighlightBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T HighlightBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.HighlightBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T HighlightBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.HighlightBrushProperty, ps, () => control.HighlightBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T HighlightBrush<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.HighlightBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T HighlightBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.HighlightBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T HighlightBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.HighlightBrushProperty, ps, () => control.HighlightBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedBrushProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedBrush<T>(this T control, Func<Avalonia.Media.IBrush> func, Action<Avalonia.Media.IBrush>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBrushProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedBrush<T>(this T control, Avalonia.Media.IBrush value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.SelectedBrushProperty, ps, () => control.SelectedBrush = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T SelectedBrush<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBrushProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedBrush<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBrushProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedBrush<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Media.IBrush> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.SelectedBrushProperty, ps, () => control.SelectedBrush = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // SelectedBorderThicknessProperty

/*BindFromExpressionSetterGenerator*/
public static T SelectedBorderThickness<T>(this T control, Func<Avalonia.Thickness> func, Action<Avalonia.Thickness>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBorderThicknessProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T SelectedBorderThickness<T>(this T control, Avalonia.Thickness value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.SelectedBorderThicknessProperty, ps, () => control.SelectedBorderThickness = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T SelectedBorderThickness<T>(this T control, System.Double uniformLength = default) where T : Nodify.ItemContainer
   => control._set(() => control.SelectedBorderThickness = new Avalonia.Thickness(uniformLength));
public static T SelectedBorderThickness<T>(this T control, System.Double horizontal = default, System.Double vertical = default) where T : Nodify.ItemContainer
   => control._set(() => control.SelectedBorderThickness = new Avalonia.Thickness(horizontal, vertical));
public static T SelectedBorderThickness<T>(this T control, System.Double left = default, System.Double top = default, System.Double right = default, System.Double bottom = default) where T : Nodify.ItemContainer
   => control._set(() => control.SelectedBorderThickness = new Avalonia.Thickness(left, top, right, bottom));

/*BindSetterGenerator*/
public static T SelectedBorderThickness<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBorderThicknessProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T SelectedBorderThickness<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.SelectedBorderThicknessProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T SelectedBorderThickness<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Thickness> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.SelectedBorderThicknessProperty, ps, () => control.SelectedBorderThickness = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSelectableProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSelectable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelectable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsSelectableProperty, ps, () => control.IsSelectable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelectable<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelectable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelectable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsSelectableProperty, ps, () => control.IsSelectable = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsSelectedProperty

/*BindFromExpressionSetterGenerator*/
public static T IsSelected<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectedProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsSelected<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsSelectedProperty, ps, () => control.IsSelected = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsSelected<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectedProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsSelected<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsSelectedProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsSelected<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsSelectedProperty, ps, () => control.IsSelected = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // LocationProperty

/*BindFromExpressionSetterGenerator*/
public static T Location<T>(this T control, Func<Avalonia.Point> func, Action<Avalonia.Point>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.LocationProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T Location<T>(this T control, Avalonia.Point value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.LocationProperty, ps, () => control.Location = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T Location<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.LocationProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T Location<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.LocationProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T Location<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Point> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.LocationProperty, ps, () => control.Location = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // ActualSizeProperty

/*BindFromExpressionSetterGenerator*/
public static T ActualSize<T>(this T control, Func<Avalonia.Size> func, Action<Avalonia.Size>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.ActualSizeProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T ActualSize<T>(this T control, Avalonia.Size value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.ActualSizeProperty, ps, () => control.ActualSize = value, bindingMode, converter, bindingSource);

/*ValueOverloadsSetterGenerator*/

public static T ActualSize<T>(this T control, System.Double width = default, System.Double height = default) where T : Nodify.ItemContainer
   => control._set(() => control.ActualSize = new Avalonia.Size(width, height));
public static T ActualSize<T>(this T control, System.Numerics.Vector2 vector2 = default) where T : Nodify.ItemContainer
   => control._set(() => control.ActualSize = new Avalonia.Size(vector2));

/*BindSetterGenerator*/
public static T ActualSize<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.ActualSizeProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T ActualSize<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.ActualSizeProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T ActualSize<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, Avalonia.Size> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.ActualSizeProperty, ps, () => control.ActualSize = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // DesiredSizeForSelectionProperty

/*BindFromExpressionSetterGenerator*/
public static T DesiredSizeForSelection<T>(this T control, Func<System.Nullable<Avalonia.Size>> func, Action<System.Nullable<Avalonia.Size>>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.DesiredSizeForSelectionProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T DesiredSizeForSelection<T>(this T control, System.Nullable<Avalonia.Size> value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.DesiredSizeForSelectionProperty, ps, () => control.DesiredSizeForSelection = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T DesiredSizeForSelection<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.DesiredSizeForSelectionProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T DesiredSizeForSelection<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.DesiredSizeForSelectionProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T DesiredSizeForSelection<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Nullable<Avalonia.Size>> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.DesiredSizeForSelectionProperty, ps, () => control.DesiredSizeForSelection = converter.TryConvert(value), bindingMode, converter, bindingSource);


 // IsDraggableProperty

/*BindFromExpressionSetterGenerator*/
public static T IsDraggable<T>(this T control, Func<System.Boolean> func, Action<System.Boolean>? onChanged = null, [CallerArgumentExpression("func")] string? expression = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsDraggableProperty, func, onChanged, expression);

/*MagicalSetterGenerator*/
public static T IsDraggable<T>(this T control, System.Boolean value, BindingMode? bindingMode = null, IValueConverter? converter = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsDraggableProperty, ps, () => control.IsDraggable = value, bindingMode, converter, bindingSource);

/*BindSetterGenerator*/
public static T IsDraggable<T>(this T control, IBinding binding) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsDraggableProperty, binding);

/*AvaloniaPropertyBindSetterGenerator*/
public static T IsDraggable<T>(this T control, AvaloniaProperty avaloniaProperty, BindingMode? bindingMode = null, IValueConverter? converter = null, ViewBase? overrideView = null) where T : Nodify.ItemContainer
   => control._set(Nodify.ItemContainer.IsDraggableProperty, avaloniaProperty, bindingMode, converter, overrideView);

/*MagicalSetterWithConverterGenerator*/
public static T IsDraggable<T,TValue>(this T control, TValue value, FuncValueConverter<TValue, System.Boolean> converter, BindingMode? bindingMode = null, object? bindingSource = null, [CallerArgumentExpression("value")] string? ps = null) where T : Nodify.ItemContainer
=> control._setEx(Nodify.ItemContainer.IsDraggableProperty, ps, () => control.IsDraggable = converter.TryConvert(value), bindingMode, converter, bindingSource);



//================= Events ======================//
 // LocationChanged

/*ActionToEventGenerator*/
    public static T OnLocationChanged<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.LocationChanged += h);


 // DragStarted

/*ActionToEventGenerator*/
    public static T OnDragStarted<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DragStarted += h);


 // DragDelta

/*ActionToEventGenerator*/
    public static T OnDragDelta<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DragDelta += h);


 // DragCompleted

/*ActionToEventGenerator*/
    public static T OnDragCompleted<T>(this T control, Action<Avalonia.Input.DragEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Input.DragEventArgs>) ((arg0, arg1) => action(arg1)), h => control.DragCompleted += h);


 // Selected

/*ActionToEventGenerator*/
    public static T OnSelected<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Selected += h);


 // Unselected

/*ActionToEventGenerator*/
    public static T OnUnselected<T>(this T control, Action<Avalonia.Interactivity.RoutedEventArgs> action) where T : Nodify.ItemContainer => 
        control._setEvent((System.EventHandler<Avalonia.Interactivity.RoutedEventArgs>) ((arg0, arg1) => action(arg1)), h => control.Unselected += h);


 // PreviewLocationChanged

/*ActionToEventGenerator*/
    public static T OnPreviewLocationChanged<T>(this T control, Action<Avalonia.Point> action) where T : Nodify.ItemContainer => 
        control._setEvent((Nodify.PreviewLocationChanged) ((arg0) => action(arg0)), h => control.PreviewLocationChanged += h);



//================= Styles ======================//
 // HighlightBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> HighlightBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.HighlightBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> HighlightBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.HighlightBrushProperty, binding);


 // SelectedBrushProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedBrush<T>(this Style<T> style, Avalonia.Media.IBrush value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.SelectedBrushProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedBrush<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.SelectedBrushProperty, binding);


 // SelectedBorderThicknessProperty

/*ValueStyleSetterGenerator*/
public static Style<T> SelectedBorderThickness<T>(this Style<T> style, Avalonia.Thickness value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.SelectedBorderThicknessProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> SelectedBorderThickness<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.SelectedBorderThicknessProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> SelectedBorderThickness<T>(this Style<T> style, System.Double uniformLength) where T : Nodify.ItemContainer
   => style._addSetter(Nodify.ItemContainer.SelectedBorderThicknessProperty, new Avalonia.Thickness(uniformLength));public static Style<T> SelectedBorderThickness<T>(this Style<T> style, System.Double horizontal, System.Double vertical) where T : Nodify.ItemContainer
   => style._addSetter(Nodify.ItemContainer.SelectedBorderThicknessProperty, new Avalonia.Thickness(horizontal, vertical));public static Style<T> SelectedBorderThickness<T>(this Style<T> style, System.Double left, System.Double top, System.Double right, System.Double bottom) where T : Nodify.ItemContainer
   => style._addSetter(Nodify.ItemContainer.SelectedBorderThicknessProperty, new Avalonia.Thickness(left, top, right, bottom));


 // IsSelectableProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelectable<T>(this Style<T> style, System.Boolean value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsSelectableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelectable<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsSelectableProperty, binding);


 // IsSelectedProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, System.Boolean value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsSelectedProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsSelected<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsSelectedProperty, binding);


 // LocationProperty

/*ValueStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, Avalonia.Point value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.LocationProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> Location<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.LocationProperty, binding);


 // ActualSizeProperty

/*ValueStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, Avalonia.Size value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.ActualSizeProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.ActualSizeProperty, binding);

/*ValueOverloadsStyleSetterGenerator*/
public static Style<T> ActualSize<T>(this Style<T> style, System.Double width, System.Double height) where T : Nodify.ItemContainer
   => style._addSetter(Nodify.ItemContainer.ActualSizeProperty, new Avalonia.Size(width, height));

public static Style<T> ActualSize<T>(this Style<T> style, System.Numerics.Vector2 vector2) where T : Nodify.ItemContainer
    => style._addSetter(Nodify.ItemContainer.ActualSizeProperty, new Avalonia.Size(vector2));


 // DesiredSizeForSelectionProperty

/*ValueStyleSetterGenerator*/
public static Style<T> DesiredSizeForSelection<T>(this Style<T> style, System.Nullable<Avalonia.Size> value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.DesiredSizeForSelectionProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> DesiredSizeForSelection<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.DesiredSizeForSelectionProperty, binding);


 // IsDraggableProperty

/*ValueStyleSetterGenerator*/
public static Style<T> IsDraggable<T>(this Style<T> style, System.Boolean value) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsDraggableProperty, value);

/*BindingStyleSetterGenerator*/
public static Style<T> IsDraggable<T>(this Style<T> style, IBinding binding) where T : Nodify.ItemContainer
=> style._addSetter(Nodify.ItemContainer.IsDraggableProperty, binding);



}
