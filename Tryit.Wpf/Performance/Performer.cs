using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

public abstract class Performer : DependencyObject
{
    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(Performer), new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(500))));

    public TimeSpan Delay
    {
        get => (TimeSpan)GetValue(DelayProperty);
        set => SetValue(DelayProperty, value);
    }

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.Register(nameof(Delay), typeof(TimeSpan), typeof(Performer), new PropertyMetadata(TimeSpan.FromMilliseconds(0)));

    public EasingFunction EasingFunction
    {
        get => (EasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(nameof(EasingFunction), typeof(EasingFunction), typeof(Performer), new PropertyMetadata(EasingFunction.Circle));

    public EasingMode EasingMode
    {
        get => (EasingMode)GetValue(EasingModeProperty);
        set => SetValue(EasingModeProperty, value);
    }

    public static readonly DependencyProperty EasingModeProperty =
         DependencyProperty.Register(nameof(EasingMode), typeof(EasingMode), typeof(Performer), new PropertyMetadata(EasingMode.EaseIn));

    public double? SpeedRatio
    {
        get => (double?)GetValue(SpeedRatioProperty);
        set => SetValue(SpeedRatioProperty, value);
    }

    public static readonly DependencyProperty SpeedRatioProperty =
        DependencyProperty.Register(nameof(SpeedRatio), typeof(double?), typeof(Performer), new PropertyMetadata(null));

    public double? DecelerationRatio
    {
        get => (double?)GetValue(DecelerationRatioProperty);
        set => SetValue(DecelerationRatioProperty, value);
    }

    public static readonly DependencyProperty DecelerationRatioProperty =
        DependencyProperty.Register(nameof(DecelerationRatio), typeof(double?), typeof(Performer), new PropertyMetadata(null));

    internal abstract AnimationTimeline CreateAnimation(DependencyObject dependencyObject);

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
    None,

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
