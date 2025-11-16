using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Animates the translation (movement) of an associated object by applying X and Y offset animations to its
/// RenderTransform.
/// </summary>
/// <remarks>Use TranslateTransition to smoothly move UI elements to a specified position by animating their
/// TranslateTransform components. This class is typically used in storyboard-based animation scenarios within custom
/// behaviors. The transition targets the X and Y axes independently, allowing for flexible movement
/// animations.</remarks>
public class TranslateTransition : TransitionBase<Point?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the TranslateTransition class with the default target position at the origin (0,
    /// 0).
    /// </summary>
    public TranslateTransition()
    {
        To = new Point(0, 0);
    }

    /// <summary>
    /// Generates the collection of DoubleAnimation objects used to animate the X and Y translation of the associated
    /// object's RenderTransform.
    /// </summary>
    /// <remarks>The generated animations target the TranslateTransform components within the associated
    /// object's RenderTransform, if present. This method is typically used to support storyboard-based animations in
    /// custom behaviors.</remarks>
    /// <returns>An enumerable collection containing the DoubleAnimation objects for the X and Y translation transforms. The
    /// collection is empty if the associated object's RenderTransform does not contain a TranslateTransform.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.X)";
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.Y)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<TranslateTransform>(out var index))
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
    /// <remarks>This method is intended to be called by the animation framework to set up individual axis
    /// animations based on the provided index. Override this method to customize how each axis is animated.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure with the animation parameters.</param>
    /// <param name="animationIndex">The index of the animation to configure. Typically, 0 corresponds to the X component and 1 to the Y component.</param>
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
/// Provides a transition that animates the X translation of an associated object's render transform using a double
/// animation.
/// </summary>
/// <remarks>Use this class to create smooth horizontal movement effects by animating the X property of a
/// TranslateTransform within a TransformGroup. The transition targets the associated object's render transform and
/// applies the animation only if a suitable TranslateTransform is present. This class is typically used in UI scenarios
/// where elements need to slide horizontally as part of a visual transition.</remarks>
public class TranslateXTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the TranslateXTransition class with the default target X position.
    /// </summary>
    public TranslateXTransition()
    {
        To = 0;
    }

    /// <summary>
    /// Generates a sequence of double animations targeting the X translation of the associated object's render
    /// transform.
    /// </summary>
    /// <remarks>This method yields animations only if the associated object's render transform contains a
    /// <see cref="TranslateTransform"/> within a <see cref="TransformGroup"/>. The returned animations are configured
    /// to target the X property of the corresponding <see cref="TranslateTransform"/>.</remarks>
    /// <returns>An enumerable collection of <see cref="DoubleAnimation"/> objects that animate the X property of a <see
    /// cref="TranslateTransform"/> within the associated object's render transform. The collection is empty if no
    /// suitable transform is found.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string XPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.X)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<TranslateTransform>(out var index))
        {
            DoubleAnimation xAnimation = new DoubleAnimation();

            Storyboard.SetTarget(xAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(xAnimation, new PropertyPath(string.Format(XPath, index)));

            yield return xAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with custom start and end values before it is applied.
    /// </summary>
    /// <remarks>Override this method to customize the animation's parameters before it is started. This
    /// method is typically called internally as part of the animation setup process.</remarks>
    /// <param name="animation">The DoubleAnimation instance to configure. The method may set its From and To properties based on the current
    /// settings.</param>
    /// <param name="animationIndex">The zero-based index of the animation being configured. Can be used to differentiate between multiple animations
    /// in a sequence.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From.HasValue ? From.Value : animation.From;
        animation.To = To.HasValue ? To.Value : animation.To;
    }
}

/// <summary>
/// Provides a transition that animates the Y translation of a UI element's render transform using a double animation.
/// </summary>
/// <remarks>Use this class to animate the vertical movement of UI elements that utilize a TranslateTransform
/// within their RenderTransform. The transition targets the Y property of the associated TranslateTransform, enabling
/// smooth vertical animations in storyboards.</remarks>
public class TranslateYTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the TranslateYTransition class with the default target Y position.
    /// </summary>
    public TranslateYTransition()
    {
        To = 0;
    }

    /// <summary>
    /// Generates a sequence of double animations targeting the Y translation of the associated object's render
    /// transform.
    /// </summary>
    /// <remarks>This method is typically used to create storyboard animations for UI elements that use a
    /// TransformGroup with a TranslateTransform. The returned animations can be used to animate the vertical position
    /// of the associated object.</remarks>
    /// <returns>An enumerable collection containing double animations for the Y property of the associated object's
    /// TranslateTransform. The collection is empty if the associated object does not have a suitable
    /// TranslateTransform.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string YPath = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(TranslateTransform.Y)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<TranslateTransform>(out var index))
        {
            DoubleAnimation yAnimation = new DoubleAnimation();

            Storyboard.SetTarget(yAnimation, AssociatedObject);

            Storyboard.SetTargetProperty(yAnimation, new PropertyPath(string.Format(YPath, index)));

            yield return yAnimation;
        }
    }

    /// <summary>
    /// Configures the specified animation with custom start and end values before it is applied.
    /// </summary>
    /// <param name="animation">The DoubleAnimation instance to configure. The method may set its From and To properties based on the current
    /// settings.</param>
    /// <param name="animationIndex">The zero-based index of the animation being configured. Can be used to differentiate between multiple animations
    /// in a sequence.</param>
    protected override void ConfigureAnimation(DoubleAnimation animation, int animationIndex)
    {
        base.ConfigureAnimation(animation, animationIndex);

        animation.From = From.HasValue ? From.Value : animation.From;
        animation.To = To.HasValue ? To.Value : animation.To;
    }
}
