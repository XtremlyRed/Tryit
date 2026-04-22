using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides a base class for objects that define animation or timing behavior using dependency properties for duration,
/// delay, easing, speed, and deceleration settings.
/// </summary>
/// <remarks>The Performer class is designed for use in animation scenarios within Windows Presentation Foundation
/// (WPF) applications. It exposes a set of dependency properties that allow customization of animation timing and
/// interpolation characteristics, such as duration, delay, easing function, speed ratio, and deceleration ratio. These
/// properties enable flexible control over how animations are performed and can be data bound, styled, or animated
/// themselves. Derived classes implement specific animation logic by extending this abstract base class.</remarks>
public abstract class Performer : DependencyObject
{
    /// <summary>
    /// Gets or sets the duration associated with the current instance.
    /// </summary>
    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>
    /// Identifies the Duration dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Duration property with the Windows
    /// Presentation Foundation (WPF) property system. Dependency properties enable styling, data binding, animation,
    /// and default value support in WPF controls.</remarks>
    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(Performer), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(500))));

    /// <summary>
    /// Gets or sets the amount of time to wait before performing the associated action.
    /// </summary>
    /// <remarks>Set this property to specify a delay interval. The behavior after changing this value may
    /// depend on the context in which the property is used. Ensure that the value is non-negative to avoid unexpected
    /// results.</remarks>
    public TimeSpan? Delay
    {
        get => (TimeSpan?)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    /// <summary>
    /// Identifies the Delay dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Delay property with the Windows Presentation
    /// Foundation (WPF) property system. It is typically used when calling methods such as SetValue or GetValue on
    /// instances of the Performer class.</remarks>
    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(TimeSpan?), typeof(Performer), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the ratio that controls the speed of the animation relative to its normal rate.
    /// </summary>
    /// <remarks>A value of 1.0 indicates the default playback speed. Values greater than 1.0 increase the
    /// speed, while values between 0.0 and 1.0 decrease it. If the value is null, the default speed is used.</remarks>
    public double? SpeedRatio
    {
        get => (double?)GetValue(SpeedRatioProperty);
        set => SetValue(SpeedRatioProperty, value);
    }

    /// <summary>
    /// Identifies the SpeedRatio dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the SpeedRatio property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of the Performer class.</remarks>
    public static readonly DependencyProperty SpeedRatioProperty =
        DependencyProperty.Register(nameof(SpeedRatio), typeof(double?), typeof(Performer), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the ratio that specifies how much the animation slows down as it approaches its final value.
    /// </summary>
    /// <remarks>A higher deceleration ratio causes the animation to slow down more noticeably near the end of
    /// its duration. The value should be between 0.0 and 1.0, where 0.0 indicates no deceleration and 1.0 indicates the
    /// animation spends all of its time decelerating. If null, a default deceleration ratio may be used.</remarks>
    public double? DecelerationRatio
    {
        get => (double?)GetValue(DecelerationRatioProperty);
        set => SetValue(DecelerationRatioProperty, value);
    }
    /// <summary>
    /// Identifies the DecelerationRatio dependency property.
    /// </summary>
    /// <remarks>This field is used to register and
    /// reference the DecelerationRatio property with the Windows Presentation Foundation
    /// (WPF) property system. It is typically used when calling methods such as SetValue,
    /// GetValue, or for property metadata operations.</remarks>
    public static readonly DependencyProperty DecelerationRatioProperty =
        DependencyProperty.Register(nameof(DecelerationRatio), typeof(double?), typeof(Performer), new PropertyMetadata(null));

    /// <summary>
    /// Builds and configures an animation timeline for the specified dependency object using the current animation
    /// settings.
    /// </summary>
    /// <param name="dependencyObject">The dependency object to which the animation will be applied. This object is used to create and configure the
    /// animation timeline.</param>
    /// <returns>An AnimationTimeline instance configured with the specified duration, delay, speed ratio, and deceleration
    /// ratio, ready to be applied to the given dependency object.</returns>
    internal AnimationTimeline AnimationBuild(DependencyObject dependencyObject)
    {
        AnimationTimeline animationTimeline = CreateAnimation(dependencyObject);

        if (DecelerationRatio.HasValue)
        {
            animationTimeline.DecelerationRatio = DecelerationRatio.Value;
        }

        if (Delay.HasValue)
        {
            animationTimeline.BeginTime = Delay.Value;
        }

        if (SpeedRatio.HasValue)
        {
            animationTimeline.SpeedRatio = SpeedRatio.Value;
        }

        animationTimeline.Duration = Duration;

        return animationTimeline;
    }

    /// <summary>
    /// Creates an instance of an AnimationTimeline that defines the animation for the specified DependencyObject.
    /// </summary>
    /// <remarks>Override this method in a derived class to provide a custom animation for the given
    /// DependencyObject. The returned AnimationTimeline determines how the property value changes over time.</remarks>
    /// <param name="dependencyObject">The object to which the animation will be applied. This parameter cannot be null.</param>
    /// <returns>An AnimationTimeline that describes the animation to apply to the specified DependencyObject.</returns>
    protected abstract AnimationTimeline CreateAnimation(DependencyObject dependencyObject);

    /// <summary>
    /// Initializes a new animation of the specified type, configures it with the provided data and callbacks, and
    /// associates it with the given dependency object and animation path.
    /// </summary>
    /// <remarks>This method is intended for internal use to streamline the setup of animations with custom
    /// configuration and event handling. The returned animation is not started automatically; callers are responsible
    /// for adding it to a storyboard or starting it as needed.</remarks>
    /// <typeparam name="T">The type of animation to create. Must derive from Animatable and have a parameterless constructor.</typeparam>
    /// <typeparam name="TData">The type of data used to configure the animation.</typeparam>
    /// <typeparam name="TTransform">The type of transform used for the animation path. Must derive from Transform and have a parameterless
    /// constructor.</typeparam>
    /// <param name="dependencyObject">The dependency object to which the animation will be applied. Cannot be null.</param>
    /// <param name="animationPath">The property path that specifies the target property for the animation. Cannot be null or empty.</param>
    /// <param name="data">The data used to configure the animation. May be null if the animation does not require additional data.</param>
    /// <param name="setCallback">A callback invoked to configure the animation with the provided data. Cannot be null.</param>
    /// <param name="initializeAnimationPathEventHandler">An optional event handler that can modify or initialize the animation path before it is applied. If null, the
    /// original animation path is used.</param>
    /// <param name="callback">An optional callback invoked with the dependency object after the animation is configured.</param>
    /// <returns>A new instance of the animation of type T, configured and associated with the specified dependency object and
    /// property path.</returns>
    internal static T Initialize<T, TData, TTransform>(DependencyObject dependencyObject, string animationPath, TData? data, Action<T, TData?> setCallback, InitializeAnimationPathEventHandler<TTransform>? initializeAnimationPathEventHandler = null, Action<DependencyObject>? callback = null)
        where T : Animatable, new()
        where TTransform : Transform, new()
    {
        T animation = new();

        setCallback?.Invoke(animation, data);
        callback?.Invoke(dependencyObject);

        if (initializeAnimationPathEventHandler is not null)
        {
            animationPath = initializeAnimationPathEventHandler.Invoke(dependencyObject, animationPath);
        }

        Storyboard.SetTargetProperty(animation, new PropertyPath(animationPath));
        Storyboard.SetTarget(animation, dependencyObject);

        return animation;
    }

    /// <summary>
    /// Represents a method that initializes an animation path for a specified dependency object and returns the
    /// resolved animation path string.
    /// </summary>
    /// <typeparam name="TTransform">The type of transform to apply. Must be a subclass of Transform with a parameterless constructor.</typeparam>
    /// <param name="dependencyObject">The dependency object for which the animation path is being initialized. Cannot be null.</param>
    /// <param name="animationPath">The animation path to initialize. This is typically a property path or identifier used for animation targeting.</param>
    /// <returns>A string representing the initialized or resolved animation path. The returned value may be used to configure or
    /// start an animation.</returns>
    internal delegate string InitializeAnimationPathEventHandler<TTransform>(DependencyObject dependencyObject, string animationPath) where TTransform : Transform, new();
}

/// <summary>
/// Specifies the types of transitions or events that can occur for a UI element, such as changes in data context, mouse
/// pointer movement, or focus state.
/// </summary>
/// <remarks>Use this enumeration to identify or handle specific UI transitions or events, such as responding to
/// focus changes or mouse interactions. The values correspond to common UI lifecycle events and can be used in event
/// handling or state management scenarios.</remarks>
public enum TransitionEvent
{
    /// <summary>
    ///
    /// </summary>
    None,

    /// <summary>
    /// Gets a value indicating whether the resource has been successfully loaded.
    /// </summary>
    Loaded,

    /// <summary>
    /// Occurs when the data context for this element changes.
    /// </summary>
    /// <remarks>This event is typically used to respond to changes in the data context, such as updating
    /// bindings or performing additional initialization when the data context is set or replaced.</remarks>
    DataContextChanged,

    /// <summary>
    /// Occurs when the mouse pointer enters the bounds of the control.
    /// </summary>
    /// <remarks>This event is typically used to provide visual feedback or initiate actions when the user
    /// moves the mouse pointer over a control. The event is raised only when the pointer enters the control's area; it
    /// is not raised again until the pointer leaves and re-enters the control.</remarks>
    MouseEnter,

    /// <summary>
    /// Occurs when the mouse pointer leaves the boundaries of the element.
    /// </summary>
    MouseLeave,

    /// <summary>
    /// Occurs when the control receives input focus.
    /// </summary>
    /// <remarks>This event is typically used to perform actions when a control becomes active, such as
    /// updating the user interface or preparing resources. The event is raised when the control receives focus either
    /// through user interaction or programmatically.</remarks>
    GotFocus,

    /// <summary>
    /// Represents the event that occurs when a control loses input focus.
    /// </summary>
    LostFocus,
}

/// <summary>
/// Specifies the type of easing function to use for interpolating values in animations.
/// </summary>
/// <remarks>Easing functions control the rate of change of an animation, allowing for effects such as
/// acceleration, deceleration, or bouncing. The selected easing function determines how the animated value progresses
/// over time, enabling more natural or stylized motion compared to linear interpolation. Choose an easing function
/// based on the desired animation effect.</remarks>
public enum EasingFunction
{
    /// <summary>
    ///
    /// </summary>
    Linear,

    /// <summary>
    /// Gets or sets the back navigation command or state for the control.
    /// </summary>
    Back,

    /// <summary>
    /// Represents the bounce animation type.
    /// </summary>
    Bounce,

    /// <summary>
    /// Represents a geometric circle defined by its center point and radius.
    /// </summary>
    /// <remarks>Use this class to perform calculations or operations related to circles, such as computing
    /// area, circumference, or determining point containment. The circle is typically defined in a two-dimensional
    /// coordinate system.</remarks>
    Circle,

    /// <summary>
    /// Specifies a cubic interpolation method or easing function.
    /// </summary>
    /// <remarks>Use this value to indicate that cubic interpolation should be applied, typically resulting in
    /// smoother transitions compared to linear methods. The specific behavior may depend on the context in which this
    /// value is used.</remarks>
    Cubic,

    /// <summary>
    /// Represents an elastic easing function used for animations that simulate a spring-like motion.
    /// </summary>
    /// <remarks>This type is typically used to create animations where the value overshoots and oscillates
    /// before settling, mimicking the behavior of a spring. It is commonly applied in UI transitions to provide a more
    /// natural and dynamic effect.</remarks>
    Elastic,

    /// <summary>
    /// Represents an exponential mathematical function or distribution.
    /// </summary>
    /// <remarks>Use this type to perform calculations or represent values related to exponential growth,
    /// decay, or probability distributions. The specific behavior depends on the context in which the type is
    /// used.</remarks>
    Exponential,

    /// <summary>
    /// Gets or sets the power level or value associated with this instance.
    /// </summary>
    Power,

    /// <summary>
    /// Represents a quadratic equation or function, typically of the form ax² + bx + c.
    /// </summary>
    /// <remarks>Use this class to model, evaluate, or solve quadratic equations. Quadratic equations are
    /// commonly used in mathematics, physics, and engineering to describe parabolic relationships.</remarks>
    Quadratic,

    /// <summary>
    /// Represents a quartic (fourth-degree) polynomial or operation involving quartic equations.
    /// </summary>
    Quartic,

    /// <summary>
    /// Represents a quintic (fifth-degree) polynomial or easing function, typically used for smooth interpolation or
    /// animation curves.
    /// </summary>
    /// <remarks>A quintic function is commonly used in animation and graphics to create smooth transitions
    /// with gradual acceleration and deceleration. This type may provide methods for evaluating the polynomial or
    /// generating easing curves for use in motion or value interpolation.</remarks>
    Quintic,

    /// <summary>
    /// Represents the sine trigonometric function.
    /// </summary>
    Sine,
}
