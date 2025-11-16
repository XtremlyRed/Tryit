using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tryit.Wpf.Internals;
using static System.Reflection.BindingFlags;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for configuring and starting property animations on WPF dependency objects in a fluent
/// manner.
/// </summary>
/// <remarks>These extension methods enable a fluent API for building and applying animations to dependency object
/// properties, such as animating colors on brushes or setting animation durations. The methods are designed to simplify
/// animation setup and improve code readability when working with WPF animation scenarios.</remarks>
public static class AnimationExtensions
{
    /// <summary>
    /// Creates an animation property selector for the specified dependency object, enabling fluent configuration of
    /// animations on its properties.
    /// </summary>
    /// <typeparam name="T">The type of the dependency object. Must derive from DependencyObject.</typeparam>
    /// <param name="dependencyObject">The dependency object for which to configure property animations. Cannot be null.</param>
    /// <returns>An AnimationPropertySelector{T} instance that can be used to specify and configure animations for the given
    /// dependency object.</returns>
    public static AnimationPropertySelector<T> Animation<T>(this T dependencyObject)
        where T : DependencyObject
    {
        return new AnimationPropertySelector<T>(dependencyObject);
    }

    /// <summary>
    /// Begins configuring and applying one or more animations to the specified dependency object using a fluent
    /// selector.
    /// </summary>
    /// <remarks>This method enables a fluent syntax for specifying multiple animations on a dependency
    /// object. The provided configuration delegate is invoked immediately to select and configure the animated
    /// properties. This method does not return a value and is intended for use with method chaining or inline animation
    /// setup.</remarks>
    /// <typeparam name="T">The type of the dependency object to animate. Must derive from DependencyObject.</typeparam>
    /// <param name="dependencyObject">The dependency object to which the animations will be applied. Cannot be null.</param>
    /// <param name="config">A delegate that receives an AnimationPropertySelector{T} to configure the properties and animations to apply.
    /// Cannot be null.</param>
    public static void BeginAnimation<T>(this T dependencyObject, Action<AnimationPropertySelector<T>> config)
        where T : DependencyObject
    {
        var builder = new AnimationPropertySelector<T>(dependencyObject);

        config?.Invoke(builder);
    }

    /// <summary>
    /// Creates a property animation builder for the Color property of a SolidColorBrush referenced by the specified
    /// Brush property on a UIElement.
    /// </summary>
    /// <remarks>The selected Brush property must reference a SolidColorBrush instance that is not frozen.
    /// Attempting to animate other Brush types or a frozen SolidColorBrush will result in an exception.</remarks>
    /// <typeparam name="T">The type of UIElement that contains the Brush property to animate.</typeparam>
    /// <param name="animationPropertySelector">An AnimationPropertySelector instance that identifies the target UIElement and provides context for the
    /// animation.</param>
    /// <param name="propertySelector">An expression that selects the Brush property to animate from the UIElement. The property must be of type
    /// SolidColorBrush.</param>
    /// <returns>A PropertyAnimationBuilder that can be used to configure and start an animation targeting the Color property of
    /// the specified SolidColorBrush.</returns>
    /// <exception cref="NotSupportedException">Thrown if the selected Brush is not a SolidColorBrush, or if the SolidColorBrush is frozen.</exception>
    public static PropertyAnimationBuilder<SolidColorBrush, Color> BrushProperty<T>(this AnimationPropertySelector<T> animationPropertySelector, Expression<Func<T, Brush>> propertySelector)
        where T : UIElement
    {
        var brush = propertySelector.Compile().Invoke(animationPropertySelector.DependencyObject);

        if (brush is not SolidColorBrush solidColorBrush)
        {
            throw new NotSupportedException("invalid brush type , must be solidcolorbrush");
        }

        if (solidColorBrush.IsFrozen)
        {
            string propertyName = AnimationExtensions.GetPropertyName(propertySelector);

            throw new NotSupportedException($"The {propertyName} object has been frozen");
        }

        var animationBuilder = solidColorBrush.Animation<SolidColorBrush>().Property<Color>(SolidColorBrush.ColorProperty);

        return animationBuilder;
    }

    /// <summary>
    /// Sets the duration of the animation in milliseconds.
    /// </summary>
    /// <typeparam name="T">The type of the object being animated. Must derive from DependencyObject.</typeparam>
    /// <typeparam name="TProperty">The type of the property being animated.</typeparam>
    /// <param name="animationBuilder">The animation builder to configure.</param>
    /// <param name="duration_ms">The duration of the animation, in milliseconds. Must be greater than 0.</param>
    /// <returns>The animation builder instance with the specified duration applied.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if duration_ms is less than or equal to 0.</exception>
    public static PropertyAnimationBuilder<T, TProperty> Duration<T, TProperty>(this PropertyAnimationBuilder<T, TProperty> animationBuilder, double duration_ms)
        where T : DependencyObject
    {
        _ = duration_ms <= 0 ? throw new ArgumentOutOfRangeException(nameof(duration_ms)) : 0;

        return animationBuilder.Duration(TimeSpan.FromMilliseconds(duration_ms));
    }

    /// <summary>
    /// Retrieves the name of the property referenced by the specified property selector expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the object containing the property.</typeparam>
    /// <typeparam name="TPropertyType">The type of the property referenced by the expression.</typeparam>
    /// <param name="propertySelector">An expression that selects the property whose name is to be retrieved. Must refer to a property of <typeparamref
    /// name="TSource"/>.</param>
    /// <returns>The name of the property referenced by the expression, or an empty string if the expression does not reference a
    /// property.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="propertySelector"/> is <see langword="null"/>.</exception>
    internal static string GetPropertyName<TSource, TPropertyType>(Expression<Func<TSource, TPropertyType>> propertySelector)
    {
        _ = propertySelector ?? throw new ArgumentNullException(nameof(propertySelector));

        if (propertySelector.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        UnaryExpression? unaryExpression = propertySelector.Body as UnaryExpression;

        return unaryExpression?.Operand is MemberExpression memberExpression2 ? memberExpression2.Member.Name : string.Empty;
    }
}
