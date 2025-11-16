using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using Tryit.Wpf.Internals;

namespace Tryit.Wpf;

/// <summary>
/// Provides a fluent builder for configuring and creating property animations on a specified dependency object and
/// dependency property.
/// </summary>
/// <remarks>Use PropertyAnimationBuilder{T, TProperty} to configure animation parameters such as duration, start
/// and end values, easing functions, and playback options before constructing an AnimationHandler{T} to execute the
/// animation. This builder supports method chaining for concise and readable animation setup. Only specific property
/// types are supported; attempting to animate an unsupported type will result in a NotSupportedException.</remarks>
/// <typeparam name="T">The type of the dependency object that will be the target of the animation. Must derive from DependencyObject.</typeparam>
/// <typeparam name="TProperty">The type of the property to animate. Must be a supported animatable type, such as numeric types, Color, Point, or
/// other compatible types.</typeparam>
public sealed class PropertyAnimationBuilder<T, TProperty>
    where T : DependencyObject
{
    /// <summary>
    /// Provides the animation setter used to apply an animation to the property value.
    /// </summary>
    /// <remarks>This field is intended for internal use to manage the animation logic for the associated
    /// property. It is not visible in the debugger due to the DebuggerBrowsable attribute.</remarks>
    [DebuggerBrowsable(Never)]
    readonly AnimationSetter<AnimationTimeline, TProperty> animationSetter;

    /// <summary>
    /// Represents a callback action to be invoked when a specific event or condition occurs.
    /// </summary>
    [DebuggerBrowsable(Never)]
    Action? callback;

    /// <summary>
    /// Represents the underlying dependency object associated with this instance.
    /// </summary>
    [DebuggerBrowsable(Never)]
    readonly T dependencyObject;

    /// <summary>
    /// Identifies the dependency property associated with this field.
    /// </summary>
    [DebuggerBrowsable(Never)]
    readonly DependencyProperty dependencyProperty;

    /// <summary>
    /// Initializes a new instance of the PropertyAnimationBuilder class for the specified dependency object and
    /// dependency property.
    /// </summary>
    /// <param name="dependencyObject">The dependency object that will be the target of the animation. Cannot be null.</param>
    /// <param name="dependencyProperty">The dependency property to animate on the specified dependency object. Cannot be null.</param>
    internal PropertyAnimationBuilder(T dependencyObject, DependencyProperty dependencyProperty)
    {
        this.dependencyObject = dependencyObject;

        this.dependencyProperty = dependencyProperty;

        animationSetter = CreateAnimationSetter();
    }

    /// <summary>
    /// Sets the duration for the property animation.
    /// </summary>
    /// <param name="duration">The length of time the animation should run. Must be a non-negative time span.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> Duration(TimeSpan duration)
    {
        animationSetter.Animation.Duration = new Duration(duration);
        return this!;
    }

    /// <summary>
    /// Specifies the starting value for the property animation.
    /// </summary>
    /// <param name="fromValue">The initial value from which the property animation will begin.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> From(TProperty fromValue)
    {
        animationSetter.FromSetter(animationSetter.Animation, fromValue);
        return this;
    }

    /// <summary>
    /// Specifies the final value of the property for the animation.
    /// </summary>
    /// <param name="toValue">The value to which the property will animate by the end of the animation.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> To(TProperty toValue)
    {
        animationSetter.ToSetter(animationSetter.Animation, toValue);
        return this;
    }

    /// <summary>
    /// Specifies the easing function to apply to the property animation.
    /// </summary>
    /// <remarks>Use this method to customize the pacing of the animation by providing a specific
    /// implementation of <see cref="IEasingFunction"/>. The easing function affects how the animated value transitions
    /// between its start and end values.</remarks>
    /// <param name="easingFunctionValue">The easing function that determines the rate of change of the animation over time. Cannot be null.</param>
    /// <returns>The current instance of <see cref="PropertyAnimationBuilder{T, TProperty}"/> to allow method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> EasingFunction(IEasingFunction easingFunctionValue)
    {
        animationSetter.EasingFunctionSetter(animationSetter.Animation, easingFunctionValue);
        return this;
    }

    /// <summary>
    /// Specifies a delay before the animation begins.
    /// </summary>
    /// <remarks>Use this method to introduce a pause before the animation starts. This is useful for
    /// sequencing multiple animations or creating staggered effects.</remarks>
    /// <param name="delay">The amount of time to wait before starting the animation. Must be non-negative.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> Delay(TimeSpan delay)
    {
        animationSetter.Animation.BeginTime = delay;
        return this!;
    }

    /// <summary>
    /// Sets the speed ratio for the animation relative to its normal playback rate.
    /// </summary>
    /// <param name="speedRatio">The multiplier applied to the animation's playback speed. A value greater than 1.0 increases the speed; a value
    /// between 0.0 and 1.0 decreases it. Must be greater than 0.0.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> SpeedRatio(double speedRatio)
    {
        animationSetter.Animation.SpeedRatio = speedRatio;
        return this!;
    }

    /// <summary>
    /// Sets the repeat behavior for the animation.
    /// </summary>
    /// <param name="repeatBehavior">The repeat behavior that determines how the animation repeats, such as a specific number of iterations or a
    /// duration.</param>
    /// <returns>The current instance of the builder to allow method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> RepeatBehavior(RepeatBehavior repeatBehavior)
    {
        animationSetter.Animation.RepeatBehavior = repeatBehavior;
        return this!;
    }

    /// <summary>
    /// Sets the fill behavior for the animation.
    /// </summary>
    /// <param name="fillBehavior">The fill behavior that determines how the animation's output value is applied before it starts and after it
    /// ends.</param>
    /// <returns>The current instance of the builder to allow method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> FillBehavior(FillBehavior fillBehavior)
    {
        animationSetter.Animation.FillBehavior = fillBehavior;
        return this!;
    }

    /// <summary>
    /// Sets the deceleration ratio for the animation, controlling how the animation slows down as it progresses.
    /// </summary>
    /// <remarks>A higher deceleration ratio causes the animation to slow down more noticeably toward the end.
    /// Setting a value outside the range 0.0 to 1.0 may result in an exception at runtime.</remarks>
    /// <param name="decelerationRatio">The deceleration ratio to apply to the animation. Must be between 0.0 and 1.0, where 0.0 specifies no
    /// deceleration and 1.0 specifies full deceleration.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> DecelerationRatio(double decelerationRatio)
    {
        animationSetter.Animation.DecelerationRatio = decelerationRatio;
        return this!;
    }

    /// <summary>
    /// Sets the acceleration ratio for the animation, specifying the percentage of the animation's duration spent
    /// accelerating from zero to full speed.
    /// </summary>
    /// <remarks>The acceleration ratio determines how quickly the animation speeds up at the beginning. A
    /// higher value results in a longer acceleration phase. The sum of acceleration and deceleration ratios should not
    /// exceed 1.0.</remarks>
    /// <param name="accelerationRatio">The proportion of the animation's total duration used for acceleration. Must be between 0.0 and 1.0, inclusive.</param>
    /// <returns>The current instance of <see cref="PropertyAnimationBuilder{T, TProperty}"/>, enabling method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> AccelerationRatio(double accelerationRatio)
    {
        animationSetter.Animation.AccelerationRatio = accelerationRatio;
        return this!;
    }

    /// <summary>
    /// Sets the name of the animation to be built.
    /// </summary>
    /// <param name="name">The name to assign to the animation. Cannot be null.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> Name(string name)
    {
        animationSetter.Animation.Name = name;
        return this!;
    }

    /// <summary>
    /// Enables or disables automatic reversal of the animation when it completes.
    /// </summary>
    /// <param name="autoReverse">true to automatically reverse the animation direction when it reaches the end; otherwise, false. The default is
    /// true.</param>
    /// <returns>The current instance of the builder to allow method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> AutoReverse(bool autoReverse = true)
    {
        animationSetter.Animation.AutoReverse = autoReverse;
        return this!;
    }

    /// <summary>
    /// Registers a callback to be invoked when the animation completes.
    /// </summary>
    /// <remarks>If multiple callbacks are registered by calling this method multiple times, only the most recent
    /// callback will be used.</remarks>
    /// <param name="callback">The action to execute when the animation has finished. Can be null if no callback is required.</param>
    /// <returns>The current <see cref="PropertyAnimationBuilder{T, TProperty}"/> instance for method chaining.</returns>
    public PropertyAnimationBuilder<T, TProperty> Completed(Action callback)
    {
        this.callback = callback;
        return this!;
    }

    /// <summary>
    /// Creates and returns a new instance of the AnimationHandler{T} class configured with the specified dependency
    /// object, dependency property, animation, and callback.
    /// </summary>
    /// <returns>An AnimationHandler{T} instance initialized with the provided parameters.</returns>
    public AnimationHandler<T> Build()
    {
        var handler = new AnimationHandler<T>(dependencyObject, dependencyProperty, animationSetter.Animation, callback);

        return handler;
    }

    /// <summary>
    /// Creates an animation setter instance appropriate for the specified property type parameter.
    /// </summary>
    /// <remarks>Supported property types include numeric types (byte, short, int, long, decimal, float,
    /// double), as well as Color, Point, Point3D, Quaternion, Rect, Rotation3D, Size, Vector, and Vector3D. Attempting
    /// to use an unsupported type will result in an exception.</remarks>
    /// <returns>An AnimationSetter instance configured for the type specified by the generic parameter TProperty.</returns>
    /// <exception cref="NotSupportedException">Thrown if the type specified by TProperty is not supported for animation.</exception>
    internal static AnimationSetter<AnimationTimeline, TProperty> CreateAnimationSetter()
    {
        var propertyType = typeof(TProperty);

        if (propertyType == typeof(byte))
        {
            return new AnimationTypeSetter<AnimationTimeline, ByteAnimation, TProperty>();
        }
        else if (propertyType == typeof(short))
        {
            return new AnimationTypeSetter<AnimationTimeline, Int16Animation, TProperty>();
        }
        else if (propertyType == typeof(int))
        {
            return new AnimationTypeSetter<AnimationTimeline, Int32Animation, TProperty>();
        }
        else if (propertyType == typeof(long))
        {
            return new AnimationTypeSetter<AnimationTimeline, Int64Animation, TProperty>();
        }
        else if (propertyType == typeof(decimal))
        {
            return new AnimationTypeSetter<AnimationTimeline, DecimalAnimation, TProperty>();
        }
        else if (propertyType == typeof(float))
        {
            return new AnimationTypeSetter<AnimationTimeline, SingleAnimation, TProperty>();
        }
        else if (propertyType == typeof(double))
        {
            return new AnimationTypeSetter<AnimationTimeline, DoubleAnimation, TProperty>();
        }
        else if (propertyType == typeof(Color))
        {
            return new AnimationTypeSetter<AnimationTimeline, ColorAnimation, TProperty>();
        }
        else if (propertyType == typeof(Point3D))
        {
            return new AnimationTypeSetter<AnimationTimeline, Point3DAnimation, TProperty>();
        }
        else if (propertyType == typeof(Point))
        {
            return new AnimationTypeSetter<AnimationTimeline, PointAnimation, TProperty>();
        }
        else if (propertyType == typeof(Quaternion))
        {
            return new AnimationTypeSetter<AnimationTimeline, QuaternionAnimation, TProperty>();
        }
        else if (propertyType == typeof(Rect))
        {
            return new AnimationTypeSetter<AnimationTimeline, RectAnimation, TProperty>();
        }
        else if (propertyType == typeof(Rotation3D))
        {
            return new AnimationTypeSetter<AnimationTimeline, Rotation3DAnimation, TProperty>();
        }
        else if (propertyType == typeof(Size))
        {
            return new AnimationTypeSetter<AnimationTimeline, SizeAnimation, TProperty>();
        }
        else if (propertyType == typeof(Vector3D))
        {
            return new AnimationTypeSetter<AnimationTimeline, Vector3DAnimation, TProperty>();
        }
        else if (propertyType == typeof(Vector))
        {
            return new AnimationTypeSetter<AnimationTimeline, VectorAnimation, TProperty>();
        }

        throw new NotSupportedException($"The property type '{propertyType.Name}' is not supported for animation.");
    }
}
