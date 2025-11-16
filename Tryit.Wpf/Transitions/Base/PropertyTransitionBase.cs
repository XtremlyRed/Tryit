using System.Diagnostics;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Xaml.Behaviors;
using Expression = System.Linq.Expressions.Expression;

namespace Tryit.Wpf;

/// <summary>
/// Base class for property-based transitions.
/// Provides a skeletal implementation that creates an animation instance and binds it
/// to the <see cref="Behavior{T}.AssociatedObject"/> and a property specified by <see cref="PropertyPath"/>.
/// </summary>
/// <typeparam name="T">The type of the associated object the transition targets (must be a DependencyObject).</typeparam>
/// <typeparam name="TAnimation">The animation timeline type used for the transition (must inherit from <see cref="AnimationTimeline"/> and have a parameterless constructor).</typeparam>
public abstract class PropertyTransitionBase<T, TAnimation> : TransitionBase<T, TAnimation>
    where TAnimation : AnimationTimeline, new()
{
    /// <summary>
    /// Gets or sets the path to the property to bind to.
    /// </summary>
    public string PropertyPath
    {
        get { return (string)GetValue(PropertyPathProperty); }
        set { SetValue(PropertyPathProperty, value); }
    }

    /// <summary>
    /// Identifies the PropertyPath dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the PropertyPath property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when interacting with property metadata,
    /// data binding, or property value inheritance for the PropertyPath property on the ColorTransition
    /// class.</remarks>
    public static readonly DependencyProperty PropertyPathProperty = DependencyProperty.Register(nameof(PropertyPath), typeof(string), typeof(ColorTransition), new PropertyMetadata(null));

    /// <summary>
    /// Creates and returns an animation configured with the associated object and property path.
    /// </summary>
    /// <returns>An enumerable collection containing a single animation instance configured for the associated object and
    /// property path.</returns>
    protected override IEnumerable<TAnimation> AnimationGenerate()
    {
        TAnimation animation = new TAnimation();

        Storyboard.SetTargetProperty(animation, new PropertyPath(PropertyPath));

        Storyboard.SetTarget(animation, AssociatedObject);

        yield return animation;
    }
}
