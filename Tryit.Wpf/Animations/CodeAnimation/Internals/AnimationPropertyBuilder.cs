using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf.Internals;

/// <summary>
/// Provides a fluent API for selecting and animating properties of a specified dependency object.
/// </summary>
/// <remarks>Use this class to create animation builders for properties of a specific dependency object. The
/// selector supports property identification by expression, property name, or dependency property. This class is
/// typically used as the entry point for configuring property animations in a type-safe manner.</remarks>
/// <typeparam name="T">The type of the dependency object whose properties can be animated. Must derive from DependencyObject.</typeparam>
public sealed class AnimationPropertySelector<T>
    where T : DependencyObject
{
    /// <summary>
    /// Gets the dependency object associated with this instance.
    /// </summary>
    public T DependencyObject { get; }

    /// <summary>
    /// Initializes a new instance of the AnimationPropertySelector class for the specified dependency object.
    /// </summary>
    /// <param name="dependencyObject">The dependency object to associate with this selector. Cannot be null.</param>
    public AnimationPropertySelector(T dependencyObject)
    {
        this.DependencyObject = dependencyObject;
    }

    /// <summary>
    /// Creates a builder for animating the specified property of the target object.
    /// </summary>
    /// <remarks>The property selector expression must refer to a public property of the target type. If the
    /// property cannot be found, an exception may be thrown at runtime.</remarks>
    /// <typeparam name="TProperty">The type of the property to animate.</typeparam>
    /// <param name="propertySelector">An expression that identifies the property to animate. Must be a member access expression, such as 'x =>
    /// x.PropertyName'.</param>
    /// <returns>A builder that can be used to configure and start an animation for the specified property.</returns>
    public PropertyAnimationBuilder<T, TProperty> Property<TProperty>(Expression<Func<T, TProperty>> propertySelector)
    {
        string propertyName = AnimationExtensions.GetPropertyName(propertySelector);

        var dpd = DependencyPropertyDescriptor.FromName(propertyName, typeof(T), typeof(T));

        var animationBuilder = new PropertyAnimationBuilder<T, TProperty>((T)DependencyObject, dpd.DependencyProperty);

        return animationBuilder;
    }

    /// <summary>
    /// Creates a builder for animating the specified property of the target object.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to animate.</typeparam>
    /// <param name="propertyName">The name of the property to animate. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <returns>A builder that can be used to configure and start an animation for the specified property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if propertyName is null, empty, or consists only of white-space characters.</exception>
    public PropertyAnimationBuilder<T, TProperty> Property<TProperty>(string propertyName)
    {
        _ = string.IsNullOrWhiteSpace(propertyName) ? throw new ArgumentNullException(nameof(propertyName)) : 0;

        var dpd = DependencyPropertyDescriptor.FromName(propertyName, typeof(T), typeof(TProperty));

        var animationBuilder = new PropertyAnimationBuilder<T, TProperty>((T)DependencyObject, dpd.DependencyProperty);

        return animationBuilder;
    }

    /// <summary>
    /// Creates a builder for animating the specified dependency property of the target object.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property to animate.</typeparam>
    /// <param name="propertySelector">The dependency property to be animated. Cannot be null.</param>
    /// <returns>A builder that can be used to configure and start an animation for the specified property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertySelector"/> is null.</exception>
    public PropertyAnimationBuilder<T, TProperty> Property<TProperty>(DependencyProperty propertySelector)
    {
        _ = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));

        var animationBuilder = new PropertyAnimationBuilder<T, TProperty>((T)DependencyObject, propertySelector);

        return animationBuilder;
    }
}
