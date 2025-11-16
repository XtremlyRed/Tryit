using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides a transition that animates the skew angles of a UI element using a SkewTransform. The SkewTransition
/// enables smooth interpolation between two skew vectors during an animation sequence.
/// </summary>
/// <remarks>SkewTransition is typically used in UI animation scenarios where a visual element needs to be skewed
/// over time, such as for visual effects or interactive feedback. It targets the AngleX and AngleY properties of a
/// SkewTransform applied to the associated element's RenderTransform. The transition supports animating both axes
/// independently and is designed to integrate with animation frameworks that utilize TransitionBase.</remarks>
public class SkewTransition : TransitionBase<Vector?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the SkewTransition class with the default target skew of (0, 0).
    /// </summary>
    public SkewTransition()
    {
        To = new Vector(0, 0);
    }

    /// <summary>
    /// Generates the collection of DoubleAnimation objects used to animate the SkewTransform angles of the associated
    /// UI element.
    /// </summary>
    /// <remarks>This method is typically called by the animation framework to retrieve the animations that
    /// will be applied to the associated UI element. The returned animations target the AngleX and AngleY properties of
    /// the SkewTransform within the element's RenderTransform, if present.</remarks>
    /// <returns>An enumerable collection containing the DoubleAnimation objects for the X and Y angles of the SkewTransform. The
    /// collection is empty if the associated object's RenderTransform does not contain a SkewTransform.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleX)";
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleY)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<SkewTransform>(out var index))
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
    /// for a two-dimensional value. It should not be called directly in most scenarios.</remarks>
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
/// Provides a transition that animates the X angle of a SkewTransform applied to a UI element, enabling skewing effects
/// along the X axis.
/// </summary>
/// <remarks>Use SkewXTransition to animate the skew angle of UI elements that have a SkewTransform within their
/// RenderTransform. The transition targets the AngleX property of the SkewTransform, allowing for smooth skew
/// animations along the horizontal axis. This class is typically used in visual state transitions or interactive UI
/// scenarios where skewing effects are desired.</remarks>
public class SkewXTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the SkewXTransition class with the default target skew angle.
    /// </summary>
    public SkewXTransition()
    {
        To = 0;
    }

    /// <summary>
    /// Generates an enumerable collection of animations that target the X angle of a SkewTransform applied to the
    /// associated UI element.
    /// </summary>
    /// <remarks>This method creates animations only if the associated object's RenderTransform is a
    /// TransformGroup containing a SkewTransform. The returned animation targets the AngleX property of the
    /// SkewTransform within the RenderTransform group.</remarks>
    /// <returns>An enumerable collection containing a DoubleAnimation for the X angle of the SkewTransform if present;
    /// otherwise, an empty collection.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleX)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<SkewTransform>(out var index))
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
    /// <param name="animation">The DoubleAnimation instance to configure. Must not be null.</param>
    /// <param name="animationIndex">The zero-based index of the animation within the sequence. Used to determine which animation is being
    /// configured.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From.HasValue ? From.Value : animation.From;
        animation.To = To.HasValue ? To.Value : animation.To;
    }
}

/// <summary>
/// Provides a transition that animates the Y-angle of a SkewTransform applied to a UI element, enabling smooth skewing
/// effects along the Y axis.
/// </summary>
/// <remarks>The SkewYTransition targets the AngleY property of the first SkewTransform found within a
/// TransformGroup assigned to the associated object's RenderTransform. This transition is typically used to create
/// visual effects where the element appears to skew or tilt vertically during an animation sequence.</remarks>
public class SkewYTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the SkewYTransition class with the default target skew angle.
    /// </summary>
    public SkewYTransition()
    {
        To = 0;
    }

    /// <summary>
    /// Generates a sequence of animations that target the Y-angle of a SkewTransform applied to the associated UI
    /// element.
    /// </summary>
    /// <remarks>The method only generates an animation if the associated object's RenderTransform is a
    /// TransformGroup containing a SkewTransform. The animation targets the AngleY property of the first SkewTransform
    /// found.</remarks>
    /// <returns>An enumerable collection containing a DoubleAnimation for the Y-angle of the SkewTransform if present;
    /// otherwise, an empty collection.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(SkewTransform.AngleY)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<SkewTransform>(out var index))
        {
            DoubleAnimation yAnimation = new DoubleAnimation();

            Storyboard.SetTarget(yAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(string.Format(YPath, index)));

            yield return yAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with custom start and end values, if provided.
    /// </summary>
    /// <remarks>Overrides the base configuration to apply custom 'From' and 'To' values if they are set. This
    /// method is typically called during the animation setup process.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure.</param>
    /// <param name="animationIndex">The zero-based index of the animation being configured.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From.HasValue ? From.Value : animation.From;
        animation.To = To.HasValue ? To.Value : animation.To;
    }
}
