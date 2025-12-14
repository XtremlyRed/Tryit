using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides functionality to perform visual transitions on UI elements using configurable transition effects and target
/// values. Supports applying transitions such as fade, scale, translate, skew, and rotate when an element appears.
/// </summary>
/// <remarks>TransitionPerformer is typically used in WPF applications to animate UI elements as they are shown,
/// allowing for customizable transition behaviors. The specific transition effect is determined by the TransitionOn
/// property, and the target value for the transition can be set using the Target property. This class leverages the WPF
/// dependency property system for data binding, animation, and styling support.</remarks>
public class TransitionPerformer : Performer
{
    /// <summary>
    /// Gets or sets the transition behavior to apply when the element appears.
    /// </summary>
    public TransitionOn TransitionOn
    {
        get => (TransitionOn)GetValue(TransitionOnProperty);
        set => SetValue(TransitionOnProperty, value);
    }

    /// <summary>
    /// Identifies the TransitionOn dependency property, which determines the transition effect to apply when showing
    /// the associated element.
    /// </summary>
    /// <remarks>This field is used when registering and referencing the TransitionOn property with the WPF
    /// property system. It is typically used in property metadata and for property system operations such as data
    /// binding, animation, and styling.</remarks>
    public static readonly DependencyProperty TransitionOnProperty = DependencyProperty.Register(nameof(TransitionOn), typeof(TransitionOn), typeof(TransitionPerformer), new PropertyMetadata(TransitionOn.FadeTo));

    /// <summary>
    /// Gets or sets the target value for the operation.
    /// </summary>
    public double? Target
    {
        get => (double?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Identifies the Target dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Target property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of TransitionPerformer.</remarks>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(double?), typeof(TransitionPerformer), new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the easing function that defines the rate of change for the animation.
    /// </summary>
    /// <remarks>Use this property to customize the acceleration and deceleration behavior of the animation.
    /// Different easing functions can create effects such as bouncing, elastic movement, or smooth
    /// transitions.</remarks>
    public EasingFunction EasingFunction
    {
        get => (EasingFunction)GetValue(EasingFunctionProperty);
        set => SetValue(EasingFunctionProperty, value);
    }

    /// <summary>
    /// Identifies the EasingFunction dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the EasingFunction property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on a Performer instance.</remarks>
    public static readonly DependencyProperty EasingFunctionProperty =
        DependencyProperty.Register(nameof(EasingFunction), typeof(EasingFunction), typeof(TransitionPerformer), new PropertyMetadata(EasingFunction.Linear));

    /// <summary>
    /// Gets or sets the easing mode that specifies how the animation interpolates values.
    /// </summary>
    /// <remarks>The easing mode determines whether the easing function is applied at the start, end, or both
    /// ends of the animation. This affects the acceleration and deceleration behavior of the animation.</remarks>
    public EasingMode EasingMode
    {
        get => (EasingMode)GetValue(EasingModeProperty);
        set => SetValue(EasingModeProperty, value);
    }

    /// <summary>
    /// Identifies the EasingMode dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the EasingMode property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of the Performer class.</remarks>
    public static readonly DependencyProperty EasingModeProperty =
         DependencyProperty.Register(nameof(EasingMode), typeof(EasingMode), typeof(TransitionPerformer), new PropertyMetadata(EasingMode.EaseIn));

    /// <summary>
    /// Creates and configures an animation timeline for the specified dependency object based on the current transition
    /// type.
    /// </summary>
    /// <remarks>The type of animation created depends on the value of the TransitionOn property. Supported
    /// transitions include fading, scaling, translation, skewing, and rotation. The dependency object must have a
    /// RenderTransform of type TransformGroup for most transitions. If the required transform is not present, the
    /// method returns null.</remarks>
    /// <param name="dependencyObject">The dependency object to which the animation will be applied. Must be a UIElement with the appropriate transform
    /// structure for the selected transition type.</param>
    /// <returns>An AnimationTimeline instance representing the configured animation for the specified transition, or null if the
    /// transition type or dependency object is not supported.</returns>
    protected override AnimationTimeline CreateAnimation(DependencyObject dependencyObject)
    {

        const string FadePath = "(UIElement.OpacityMask).(SolidColorBrush.Color)";
        const string RotatePath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(RotateTransform.Angle)";
        const string ScaleXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleX)";
        const string ScaleYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleY)";
        const string TranslateXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.X)";
        const string TranslateYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.Y)";
        const string SkewXPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleX)";
        const string SkewYPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleY)";

        if (TransitionOn is TransitionOn.FadeFrom)
        {
            return Initialize<ColorAnimation, double?, MatrixTransform>(dependencyObject, FadePath, Target, (animation, data) => animation.From = data.HasValue ? Color.FromArgb((byte)((data!.Value < 0 ? 0 : data.Value > 1 ? 1 : data.Value) * 255), 0, 0, 0) : null, null, s => ((FrameworkElement)s).OpacityMask ??= new SolidColorBrush(Colors.Black));
        }

        if (TransitionOn is TransitionOn.FadeTo)
        {
            return Initialize<ColorAnimation, double?, MatrixTransform>(dependencyObject, FadePath, Target, (animation, data) => animation.To = data.HasValue ? Color.FromArgb((byte)((data!.Value < 0 ? 0 : data.Value > 1 ? 1 : data.Value) * 255), 0, 0, 0) : null, null, s => ((FrameworkElement)s).OpacityMask ??= new SolidColorBrush(Colors.Black));
        }

        if (dependencyObject is not UIElement uIElement || uIElement.RenderTransform is not TransformGroup transformGroup)
        {
            return null!;
        }

        DoubleAnimation doubleAnimation = default!;

        if (TransitionOn is TransitionOn.ScaleXTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);
        }
        else if (TransitionOn is TransitionOn.ScaleYTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);//
        }
        else if (TransitionOn is TransitionOn.ScaleXFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);
        }
        else if (TransitionOn is TransitionOn.ScaleYFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, ScaleTransform>(dependencyObject, ScaleYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<ScaleTransform>);//
        }
        else if (TransitionOn is TransitionOn.TranslateXTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);
        }
        else if (TransitionOn is TransitionOn.TranslateYTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);//
        }
        else if (TransitionOn is TransitionOn.TranslateXFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);
        }
        else if (TransitionOn is TransitionOn.TranslateYFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, TranslateTransform>(dependencyObject, TranslateYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<TranslateTransform>);//
        }
        else if (TransitionOn is TransitionOn.SkewXTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewXPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<SkewTransform>);
        }
        else if (TransitionOn is TransitionOn.SkewYTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewYPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<SkewTransform>);//
        }
        else if (TransitionOn is TransitionOn.SkewXFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewXPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<SkewTransform>);
        }
        else if (TransitionOn is TransitionOn.SkewYFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, SkewTransform>(dependencyObject, SkewYPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<SkewTransform>);//
        }
        else if (TransitionOn is TransitionOn.RotateFrom)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, RotateTransform>(dependencyObject, RotatePath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null, Initialize<RotateTransform>);
        }
        else if (TransitionOn is TransitionOn.RotateTo)
        {
            doubleAnimation = Initialize<DoubleAnimation, double?, RotateTransform>(dependencyObject, RotatePath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null, Initialize<RotateTransform>);//
        }

        _ = (doubleAnimation?.EasingFunction = EasingFunction.WithEasing(EasingMode));

        return doubleAnimation!;

        static string Initialize<TTransform>(DependencyObject dependencyObject, string animationPath)
            where TTransform : Transform, new()
        {
            if (dependencyObject is UIElement uIElement && uIElement.RenderTransform is TransformGroup transformGroup)
            {
                if (transformGroup.TryIndexOf<TTransform>(out var index))
                {
                    animationPath = string.Format(animationPath, index);
                }
            }
            return animationPath;
        }
    }
}

/// <summary>
/// Specifies the types of transition animations that can be applied to a visual element, such as fading, rotating,
/// scaling, translating, or skewing along various axes.
/// </summary>
/// <remarks>Use the members of this enumeration to indicate the desired animation effect when transitioning a
/// visual element between states or positions. Each value represents a distinct animation type or direction, allowing
/// for fine-grained control over how elements appear, move, or transform during transitions. The specific effect and
/// its parameters (such as target values or angles) depend on the selected member and the animation framework in
/// use.</remarks>
public enum TransitionOn
{
    /// <summary>
    /// Gets or sets the target opacity value to which the element will fade.
    /// </summary>
    FadeTo,
    /// <summary>
    /// Gets or sets the starting opacity value for the fade animation.
    /// </summary>
    FadeFrom,
    /// <summary>
    /// Gets or sets the target rotation angle, in degrees, to which the object should be rotated.
    /// </summary>
    RotateTo,
    /// <summary>
    /// Gets or sets the starting angle, in degrees, from which the rotation is applied.
    /// </summary>
    RotateFrom,
    /// <summary>
    /// Gets or sets the target horizontal scale factor for the scaling operation.
    /// </summary>
    ScaleXTo,
    /// <summary>
    /// Gets or sets the target scale factor along the Y-axis for the scaling animation.
    /// </summary>
    ScaleYTo,

    /// <summary>
    /// Gets or sets the starting horizontal scale value for the animation.
    /// </summary>
    ScaleXFrom,
    /// <summary>
    /// Gets or sets the starting scale factor along the Y-axis for the animation.
    /// </summary>
    ScaleYFrom,

    /// <summary>
    /// Gets or sets the target horizontal translation value for the element's X position animation.
    /// </summary>
    /// <remarks>Set this property to specify the final X coordinate, in device-independent units (pixels), to
    /// which the element will animate. The animation will move the element horizontally from its current position to
    /// the specified value.</remarks>
    TranslateXTo,

    /// <summary>
    /// Gets or sets the target Y-axis translation value for the associated element's animation.
    /// </summary>
    TranslateYTo,
    /// <summary>
    /// Gets or sets the starting horizontal translation value for the animation, in device-independent units (DIU).
    /// </summary>
    /// <remarks>This property defines the initial X position from which the animated element will begin its
    /// translation. The value is typically specified in pixels, where positive values move the element to the right and
    /// negative values move it to the left.</remarks>
    TranslateXFrom,
    /// <summary>
    /// Gets or sets the starting vertical translation value for an animation, in device-independent units (DIU).
    /// </summary>
    /// <remarks>Use this property to specify the initial Y-axis offset from which the animated element begins
    /// its translation. The value is typically measured in pixels, where positive values move the element downward and
    /// negative values move it upward.</remarks>
    TranslateYFrom,
    /// <summary>
    /// Represents an animation that applies a skew transformation along the X-axis to a target element over a specified
    /// duration.
    /// </summary>
    SkewXTo,
    /// <summary>
    /// Gets or sets the target Y-axis skew angle for the transformation, in degrees.
    /// </summary>
    SkewYTo,
    /// <summary>
    /// Gets or sets the starting angle, in degrees, for the X-axis skew transformation applied to the element's
    /// animation.
    /// </summary>
    SkewXFrom,
    /// <summary>
    /// Gets or sets the starting angle, in degrees, for the Y-axis skew transformation applied to the object.
    /// </summary>
    SkewYFrom,
}
