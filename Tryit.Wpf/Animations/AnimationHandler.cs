using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides functionality to manage and play an animation on a specified dependency property of a WPF dependency
/// object.
/// </summary>
/// <remarks>AnimationHandler is intended for use with WPF objects that support property animation, such as
/// UIElement or Animatable. The handler manages the association between the target object, the dependency property, and
/// the animation timeline, and can optionally invoke a callback when the animation completes.</remarks>
/// <typeparam name="T">The type of the dependency object whose property will be animated. Must derive from DependencyObject.</typeparam>
public sealed class AnimationHandler<T>
    where T : DependencyObject
{
    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly Action? callback;

    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly T dependencyObject;

    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly DependencyProperty dependencyProperty;

    /// <summary>
    ///
    /// </summary>
    [DebuggerBrowsable(Never)]
    private readonly AnimationTimeline animationTimeline;

    /// <summary>
    /// Initializes a new instance of the AnimationHandler class to manage an animation on a specified dependency
    /// property.
    /// </summary>
    /// <remarks>If a callback is provided, it will be invoked when the animation timeline completes. The
    /// animation handler subscribes to the Completed event of the animation timeline only if a callback is
    /// specified.</remarks>
    /// <param name="dependencyObject">The object whose dependency property will be animated. Cannot be null.</param>
    /// <param name="dependencyProperty">The dependency property to animate. Cannot be null.</param>
    /// <param name="animationTimeline">The animation timeline that defines the animation behavior. Cannot be null.</param>
    /// <param name="callback">An optional action to invoke when the animation completes. If null, no callback is registered.</param>
    internal AnimationHandler(T dependencyObject, DependencyProperty dependencyProperty, AnimationTimeline animationTimeline, Action? callback)
    {
        this.dependencyObject = dependencyObject;
        this.dependencyProperty = dependencyProperty;
        this.animationTimeline = animationTimeline;

        if ((this.callback = callback) is not null)
        {
            animationTimeline.Completed += OnCompleted;
        }
    }

    /// <summary>
    /// Handles the completion event of the animation timeline and invokes the associated callback, if any.
    /// </summary>
    /// <param name="sender">The source of the event. This parameter is typically the animation timeline that has completed.</param>
    /// <param name="e">An object that contains the event data.</param>
    private void OnCompleted(object? sender, EventArgs e)
    {
        animationTimeline.Completed -= OnCompleted;

        callback?.Invoke();
    }

    /// <summary>
    /// Begins playback of the associated animation on the specified dependency property.
    /// </summary>
    public void Play()
    {
        InnerPlay(dependencyObject, dependencyProperty, animationTimeline);
    }

    /// <summary>
    /// Applies the specified animation to a dependency property on the given source object if it supports animation.
    /// </summary>
    /// <remarks>This method attempts to begin an animation on the source object if it is compatible with WPF
    /// animation (UIElement or Animatable). If the source does not support animation, the method has no
    /// effect.</remarks>
    /// <typeparam name="Tr">The type of the source object. Must be either UIElement or Animatable to support animation.</typeparam>
    /// <param name="source">The object to which the animation is applied. Must be a UIElement or Animatable; otherwise, no animation is
    /// performed.</param>
    /// <param name="dependencyProperty">The dependency property to animate on the source object.</param>
    /// <param name="animationTimeline">The animation to apply to the specified dependency property.</param>
    private static void InnerPlay<Tr>(Tr source, DependencyProperty dependencyProperty, AnimationTimeline animationTimeline)
    {
        if (source is UIElement uIElement)
        {
            uIElement.BeginAnimation(dependencyProperty, animationTimeline);
        }
        else if (source is Animatable animatable)
        {
            animatable.BeginAnimation(dependencyProperty, animationTimeline);
        }
    }
}
