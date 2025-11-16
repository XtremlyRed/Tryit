using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

public delegate void PropertyChangedCallback<TObject, TProperty>(TObject d, TProperty newValue, TProperty oldValue);

public class FrameworkPropertyMetadata<TObject, TProperty> : System.Windows.FrameworkPropertyMetadata
{
    public FrameworkPropertyMetadata() { }

    public FrameworkPropertyMetadata(TProperty defaultValue)
        : base(defaultValue) { }

    public FrameworkPropertyMetadata(PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base((s, e) => Callback(s, e, propertyChangedCallback)) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags)
        : base(defaultValue, flags) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base(defaultValue, (s, e) => Callback(s, e, propertyChangedCallback)) { }

    public FrameworkPropertyMetadata(PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base((s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback)) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base(defaultValue, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback, isAnimationProhibited) { }

    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited, UpdateSourceTrigger defaultUpdateSourceTrigger)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback, isAnimationProhibited, defaultUpdateSourceTrigger) { }

    private static void Callback<TO, TP>(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs, PropertyChangedCallback<TO, TP?> propertyChangedCallback)
    {
        if (propertyChangedCallback is null)
        {
            return;
        }

        if (dependencyObject is not TO tobject)
        {
            return;
        }

        TP? newValue = Changed<TP>(dependencyPropertyChangedEventArgs.NewValue);
        TP? oldValue = Changed<TP>(dependencyPropertyChangedEventArgs.OldValue);
        propertyChangedCallback(tobject, newValue, oldValue);
    }

    private static T Changed<T>(object value)
    {
        return value is T target ? target : default!;
    }
}
