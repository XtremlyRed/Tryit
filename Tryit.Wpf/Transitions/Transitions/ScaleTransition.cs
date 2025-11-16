using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides a transition that animates the scaling (X and Y axes) of an associated object using double animations.
/// </summary>
/// <remarks>Use ScaleTransition to apply smooth scaling effects to UI elements by animating their ScaleTransform
/// properties. This class targets both the X and Y scale components, enabling two-dimensional scaling animations. The
/// default scaling is set to (1, 1), representing no scale change. ScaleTransition is typically used in scenarios where
/// elements need to grow, shrink, or otherwise animate their size as part of a visual transition.</remarks>
public class ScaleTransition : TransitionBase<Vector?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ScaleTransition class with default scaling values.
    /// </summary>
    /// <remarks>The To property is initialized to a scale of (1, 1), representing no scaling by
    /// default.</remarks>
    public ScaleTransition()
    {
        To = new Vector(1, 1);
    }

    /// <summary>
    /// Generates a sequence of animations that target the X and Y scale transforms of the associated object.
    /// </summary>
    /// <remarks>The returned animations are configured to target the ScaleX and ScaleY properties of the
    /// first matching ScaleTransform within the associated object's RenderTransform group. This method is typically
    /// used to apply scaling animations in both dimensions simultaneously.</remarks>
    /// <returns>An enumerable collection containing the X and Y scale animations for the associated object's transform group.
    /// The collection is empty if no scale transform is found.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleX)";
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleY)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<ScaleTransform>(out var index))
        {
            DoubleAnimation xAnimation = new DoubleAnimation();

            DoubleAnimation yAnimation = new DoubleAnimation();

            Storyboard.SetTarget(xAnimation, AssociatedObject);

            Storyboard.SetTarget(yAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(string.Format(XPath, index)));

            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(string.Format(YPath, index)));

            yield return xAnimation;

            yield return yAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with the appropriate start and end values for the given animation index.
    /// </summary>
    /// <remarks>This method is typically called by the animation system to set up individual axis animations
    /// for a two-dimensional value. Override this method to customize how each axis is animated.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure. The method sets its From and To properties based on the animation
    /// index.</param>
    /// <param name="animationIndex">The index of the animation to configure. An index of 0 configures the X component; an index of 1 configures the
    /// Y component.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        if (animationIndex == 0)
        {
            animation.From = From.HasValue ? From.Value.X : animation.From;
            animation.To = To.HasValue ? To.Value.X : animation.To;
        }
        else if (animationIndex == 1)
        {
            animation.From = From.HasValue ? From.Value.Y : animation.From;
            animation.To = To.HasValue ? To.Value.Y : animation.To;
        }
    }
}

/// <summary>
/// Provides a transition that animates the ScaleX property of a ScaleTransform within a UI element's RenderTransform
/// group.
/// </summary>
/// <remarks>By default, the target scale value is set to 1, resulting in no scaling along the X axis unless a
/// different value is specified. This transition is typically used to smoothly animate horizontal scaling effects on UI
/// elements that utilize a ScaleTransform in their RenderTransform hierarchy.</remarks>
public class ScaleXTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ScaleXTransition class with the default target scale value.
    /// </summary>
    /// <remarks>The To property is set to 1 by default, representing no scaling along the X axis unless
    /// otherwise specified.</remarks>
    public ScaleXTransition()
    {
        To = 1;
    }

    /// <summary>
    /// Generates a sequence of double animations targeting the ScaleX property of a ScaleTransform within the
    /// associated object's RenderTransform group.
    /// </summary>
    /// <remarks>This method only yields an animation if the associated object's RenderTransform is a
    /// TransformGroup containing a ScaleTransform. The animation targets the ScaleX property of the first matching
    /// ScaleTransform found.</remarks>
    /// <returns>An enumerable collection containing a DoubleAnimation for the ScaleX property if a ScaleTransform is present;
    /// otherwise, an empty collection.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleX)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<ScaleTransform>(out var index))
        {
            DoubleAnimation xAnimation = new DoubleAnimation();

            Storyboard.SetTarget(xAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(string.Format(XPath, index)));

            yield return xAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with custom start and end values for the animation sequence.
    /// </summary>
    /// <remarks>Overrides the base configuration to apply custom From and To values if they are specified.
    /// This method is typically called internally during the animation setup process.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure. This parameter cannot be null.</param>
    /// <param name="animationIndex">The zero-based index of the animation within the sequence.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From ?? animation.From;
        animation.To = To ?? animation.To;
    }
}

/// <summary>
/// Provides a transition that animates the vertical scale (ScaleY) of a UI element using a DoubleAnimation targeting
/// the ScaleTransform within the element's RenderTransform group.
/// </summary>
/// <remarks>Use this class to create smooth vertical scaling effects for UI elements that utilize a
/// TransformGroup containing a ScaleTransform. The transition automatically targets the ScaleY property if a suitable
/// ScaleTransform is present in the associated object's RenderTransform. If no ScaleTransform is found, the transition
/// has no effect.</remarks>
public class ScaleYTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ScaleYTransition class with the default target scale value.
    /// </summary>
    public ScaleYTransition()
    {
        To = 1;
    }

    /// <summary>
    /// Generates a sequence of DoubleAnimation objects that target the ScaleY property of a ScaleTransform within the
    /// associated object's RenderTransform group.
    /// </summary>
    /// <remarks>This method is typically used to create animations for vertical scaling effects on UI
    /// elements that use a TransformGroup containing a ScaleTransform. If the associated object's RenderTransform does
    /// not contain a ScaleTransform, no animations are generated.</remarks>
    /// <returns>An enumerable collection containing a DoubleAnimation for the ScaleY property if a ScaleTransform is present;
    /// otherwise, an empty collection.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(ScaleTransform.ScaleY)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<ScaleTransform>(out var index))
        {
            DoubleAnimation yAnimation = new DoubleAnimation();

            Storyboard.SetTarget(yAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(string.Format(YPath, index)));

            yield return yAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with custom start and end values for the animation sequence.
    /// </summary>
    /// <remarks>Override this method to customize the animation parameters for each animation in a sequence.
    /// This method is typically called internally during animation setup.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure. The method may set its From and To properties based on the current
    /// configuration.</param>
    /// <param name="animationIndex">The zero-based index of the animation within the sequence. Used to identify which animation is being configured.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From ?? animation.From;
        animation.To = To ?? animation.To;
    }
}
