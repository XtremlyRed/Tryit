using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Tryit.Wpf;

/// <summary>
/// Represents a method that is called when a property value changes on a specified object.
/// </summary>
/// <remarks>Use this delegate to respond to property changes, such as updating dependent values or triggering
/// side effects. The callback provides both the old and new values, allowing for comparison or conditional logic based
/// on the change.</remarks>
/// <typeparam name="TObject">The type of the object on which the property value has changed.</typeparam>
/// <typeparam name="TProperty">The type of the property whose value has changed.</typeparam>
/// <param name="d">The object whose property value has changed.</param>
/// <param name="newValue">The new value of the property after the change.</param>
/// <param name="oldValue">The previous value of the property before the change.</param>
public delegate void PropertyChangedCallback<TObject, TProperty>(TObject d, TProperty newValue, TProperty oldValue);

/// <summary>
/// Provides strongly-typed metadata for a dependency property, including default value, property change notification,
/// coercion, and additional property behavior options, for use with the WPF property system.
/// </summary>
/// <remarks>Use FrameworkPropertyMetadata{TObject, TProperty} to define metadata for a dependency property with
/// compile-time type safety for property change callbacks. This class enables specifying default values, property
/// change and coercion callbacks, and property behavior flags in a type-safe manner, reducing the need for casting and
/// improving code clarity when registering dependency properties in WPF.</remarks>
/// <typeparam name="TObject">The type of the object that owns the dependency property. Must derive from DependencyObject.</typeparam>
/// <typeparam name="TProperty">The type of the dependency property's value.</typeparam>
public class FrameworkPropertyMetadata<TObject, TProperty> : System.Windows.FrameworkPropertyMetadata
{
    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with default property metadata settings.
    /// </summary>
    /// <remarks>This constructor creates a FrameworkPropertyMetadata object with all properties set to their
    /// default values. Use this overload when you do not need to specify any custom metadata options for a dependency
    /// property.</remarks>
    public FrameworkPropertyMetadata() { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value for the
    /// property.
    /// </summary>
    /// <param name="defaultValue">The default value to assign to the property. This value is used when no other value is set.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue)
        : base(defaultValue) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with a specified callback to be invoked when
    /// the property value changes.
    /// </summary>
    /// <param name="propertyChangedCallback">A delegate that is called when the property's value changes. The callback receives the object on which the
    /// property changed and the new value. Can be null if no callback is required.</param>
    public FrameworkPropertyMetadata(PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base((s, e) => Callback(s, e, propertyChangedCallback)) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class using the specified default value and property
    /// metadata options.
    /// </summary>
    /// <param name="defaultValue">The default value to be assigned to the dependency property.</param>
    /// <param name="flags">A combination of FrameworkPropertyMetadataOptions values that specify property behavior options.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags)
        : base(defaultValue, flags) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value and property
    /// changed callback.
    /// </summary>
    /// <param name="defaultValue">The default value to be assigned to the property when no other value is set.</param>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property's value changes. Receives the object and the new property value as
    /// parameters.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base(defaultValue, (s, e) => Callback(s, e, propertyChangedCallback)) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified property changed and coerce
    /// value callbacks.
    /// </summary>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property's value changes. The callback receives the object and the new
    /// property value as parameters.</param>
    /// <param name="coerceValueCallback">A callback that is invoked to coerce the property's value before it is set. Can be null if no coercion is
    /// required.</param>
    public FrameworkPropertyMetadata(PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base((s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value, property
    /// metadata options, and a callback to invoke when the property value changes.
    /// </summary>
    /// <param name="defaultValue">The default value to assign to the property if no other value is provided. This value is used when the property
    /// is first initialized.</param>
    /// <param name="flags">A bitwise combination of FrameworkPropertyMetadataOptions values that specify property behavior, such as whether
    /// the property is inheritable or affects rendering.</param>
    /// <param name="propertyChangedCallback">A delegate to be invoked when the property value changes. The callback receives the object on which the property
    /// changed and the new value. Can be null if no callback is required.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback)) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value, property
    /// changed callback, and coerce value callback.
    /// </summary>
    /// <param name="defaultValue">The default value to be assigned to the property if no other value is provided.</param>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property's value changes. Receives the object and the new value as
    /// parameters.</param>
    /// <param name="coerceValueCallback">A callback that is used to coerce the value of the property before it is set.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base(defaultValue, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value, property
    /// metadata options, property changed callback, and coerce value callback.
    /// </summary>
    /// <param name="defaultValue">The default value of the property for which this metadata is being created.</param>
    /// <param name="flags">A bitwise combination of FrameworkPropertyMetadataOptions values that specify property behavior options, such as
    /// whether the property is inheritable or affects rendering.</param>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property value changes. The callback receives the object and the new
    /// property value as parameters.</param>
    /// <param name="coerceValueCallback">A callback that is invoked to coerce the value of the property before it is set. This allows custom logic to
    /// adjust or validate the value.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value, property
    /// metadata options, property changed callback, coerce value callback, and animation prohibition setting.
    /// </summary>
    /// <param name="defaultValue">The default value of the property for which this metadata is being applied.</param>
    /// <param name="flags">A combination of FrameworkPropertyMetadataOptions values that specify property behavior, such as whether the
    /// property affects rendering or inherits values.</param>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property value changes. The callback receives the object and the new
    /// property value as parameters. Can be null if no callback is required.</param>
    /// <param name="coerceValueCallback">A callback that is invoked to coerce the property value before it is set. Can be null if no coercion is
    /// required.</param>
    /// <param name="isAnimationProhibited">true to prohibit animation of the property; otherwise, false.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback, isAnimationProhibited) { }

    /// <summary>
    /// Initializes a new instance of the FrameworkPropertyMetadata class with the specified default value, property
    /// options, change and coercion callbacks, animation prohibition, and default update source trigger.
    /// </summary>
    /// <param name="defaultValue">The default value of the property. This value is used when no other value is set.</param>
    /// <param name="flags">A combination of FrameworkPropertyMetadataOptions values that specify property behavior, such as whether the
    /// property affects layout or inherits values.</param>
    /// <param name="propertyChangedCallback">A callback that is invoked when the property value changes. Can be null if no callback is required.</param>
    /// <param name="coerceValueCallback">A callback that is invoked to coerce the property value before it is set. Can be null if no coercion is needed.</param>
    /// <param name="isAnimationProhibited">true to prohibit animation of the property; otherwise, false.</param>
    /// <param name="defaultUpdateSourceTrigger">The default UpdateSourceTrigger value that determines when the binding source is updated.</param>
    public FrameworkPropertyMetadata(TProperty defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback<TObject, TProperty?> propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited, UpdateSourceTrigger defaultUpdateSourceTrigger)
        : base(defaultValue, flags, (s, e) => Callback(s, e, propertyChangedCallback), coerceValueCallback, isAnimationProhibited, defaultUpdateSourceTrigger) { }

    /// <summary>
    /// Invokes a strongly-typed property changed callback when a dependency property's value changes.
    /// </summary>
    /// <remarks>The callback is only invoked if the provided dependency object can be cast to the specified
    /// type parameter TO. If propertyChangedCallback is null, no action is taken.</remarks>
    /// <typeparam name="TO">The type of the object that owns the dependency property. Must be a DependencyObject.</typeparam>
    /// <typeparam name="TP">The type of the dependency property's value.</typeparam>
    /// <param name="dependencyObject">The object on which the dependency property value has changed.</param>
    /// <param name="dependencyPropertyChangedEventArgs">The event data that contains information about the property change, including the old and new values.</param>
    /// <param name="propertyChangedCallback">A callback to invoke when the property value changes. Receives the typed object, the new value, and the old
    /// value. If null, the callback is not invoked.</param>
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

    /// <summary>
    /// Attempts to cast the specified object to the specified type and returns the result, or the default value of the
    /// type if the cast is not possible.
    /// </summary>
    /// <remarks>If the specified object is not of type T, the method returns the default value for T, which
    /// is null for reference types and the default zero-initialized value for value types.</remarks>
    /// <typeparam name="T">The type to which to attempt to cast the object.</typeparam>
    /// <param name="value">The object to cast to type T.</param>
    /// <returns>The object cast to type T if the cast is successful; otherwise, the default value for type T.</returns>
    private static T Changed<T>(object value)
    {
        return value is T target ? target : default!;
    }
}
