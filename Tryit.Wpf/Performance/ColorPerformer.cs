using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a performer that animates color properties of a WPF control based on the specified state and target
/// color.
/// </summary>
/// <remarks>ColorPerformer is typically used to animate color transitions for properties such as background,
/// border, foreground, fill, or stroke in response to control state changes. It leverages the WPF dependency property
/// system to enable data binding, styling, and animation. This class is intended for use within WPF applications and
/// requires the appropriate dependency properties to be set for correct operation.</remarks>
public class ColorPerformer : Performer
{
    /// <summary>
    /// Gets or sets the color to display when the control is in the 'on' state.
    /// </summary>
    public ColorOn ColorOn
    {
        get => (ColorOn)GetValue(ColorOnProperty);
        set => SetValue(ColorOnProperty, value);
    }

    /// <summary>
    /// Identifies the ColorOn dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the ColorOn property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue,
    /// GetValue, or for property metadata registration.</remarks>
    public static readonly DependencyProperty ColorOnProperty =
        DependencyProperty.Register(nameof(ColorOn), typeof(ColorOn), typeof(ColorPerformer), new PropertyMetadata(ColorOn.BackgroundTo));

    /// <summary>
    /// Gets or sets the target color value.
    /// </summary>
    public Color? Target
    {
        get => (Color?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Identifies the Target dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Target property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of ColorPerformer.</remarks>
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(Color?), typeof(ColorPerformer), new PropertyMetadata(null));

    /// <summary>
    /// Creates and configures a color animation for the specified dependency object based on the current color
    /// transition settings.
    /// </summary>
    /// <remarks>The specific color property animated (such as background, border brush, foreground, fill, or
    /// stroke) and whether the animation targets the 'From' or 'To' value are determined by the current value of the
    /// ColorOn property. This method is typically used in custom animation scenarios where color transitions are
    /// required for WPF elements.</remarks>
    /// <param name="dependencyObject">The dependency object to which the color animation will be applied. Must not be null.</param>
    /// <returns>An AnimationTimeline that animates the color property of the specified dependency object according to the
    /// configured color transition. Returns null if the color transition type is not supported.</returns>
    protected override AnimationTimeline CreateAnimation(DependencyObject dependencyObject)
    {
        const string ColorPath = "({0}.{1}).(SolidColorBrush.Color)";

        var colorOn = ColorOn.ToString().Replace("From", "").Replace("To", "");

        var animationPath = string.Format(ColorPath, dependencyObject.GetType().Name, colorOn);

        if (ColorOn is ColorOn.BackgroundFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BackgroundTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BorderBrushFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.BorderBrushTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.ForegroundFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.ForegroundTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.FillFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.FillTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);
        }
        if (ColorOn is ColorOn.StrokeFrom)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.From = data.HasValue ? data.Value : null);  //
        }
        if (ColorOn is ColorOn.StrokeTo)
        {
            return Initialize<ColorAnimation, Color?, MatrixTransform>(dependencyObject, animationPath, Target, (animation, data) => animation.To = data.HasValue ? data.Value : null);  //
        }

        //返回
        return default!;
    }
}

/// <summary>
/// Specifies the visual element or property to which a color or brush is applied, typically in the context of
/// gradients, animations, or drawing operations.
/// </summary>
/// <remarks>Use this enumeration to indicate the target or source of a color or brush operation, such as the
/// starting or ending color in a gradient, or the brush applied to a border or foreground during an animation. The
/// values are commonly used in UI frameworks to control visual transitions and rendering behaviors.</remarks>
public enum ColorOn
{
    /// <summary>
    /// Gets or sets the starting color of the background gradient.
    /// </summary>
    BackgroundFrom,

    /// <summary>
    /// Gets or sets the background color to transition to during an animation.
    /// </summary>
    BackgroundTo,
    /// <summary>
    /// Gets or sets the brush used to draw the border on the starting edge of the element.
    /// </summary>
    BorderBrushFrom,
    /// <summary>
    /// Gets or sets the target brush to apply to the border during the animation.
    /// </summary>
    BorderBrushTo,

    /// <summary>
    /// Gets or sets the foreground color to use as the starting value in a gradient or color transition.
    /// </summary>
    ForegroundFrom,
    /// <summary>
    /// Gets or sets the target foreground color to apply.
    /// </summary>
    ForegroundTo,
    /// <summary>
    /// Gets or sets the source from which to fill data or values.
    /// </summary>
    FillFrom,
    /// <summary>
    /// Gets or sets a value indicating whether the element should expand to fill the available space in its parent
    /// container.
    /// </summary>
    /// <remarks>Set this property to enable the element to automatically adjust its size to occupy any
    /// remaining space within its parent. This is commonly used in layout scenarios where flexible sizing is
    /// required.</remarks>
    FillTo,
    /// <summary>
    /// Gets or sets the starting point of the stroke.
    /// </summary>
    StrokeFrom,

    /// <summary>
    /// Represents a drawing command that creates a straight line from the current point to the specified coordinates
    /// without lifting the pen.
    /// </summary>
    StrokeTo,
}