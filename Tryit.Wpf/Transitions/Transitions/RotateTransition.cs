using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides a transition that animates the rotation angle of a RotateTransform within the associated object's render
/// transform group.
/// </summary>
/// <remarks>Use this class to apply a smooth rotation animation to UI elements that have a RotateTransform as
/// part of their RenderTransform. The transition targets the angle property of the first RotateTransform found in a
/// TransformGroup. If no suitable RotateTransform is present, the transition has no effect.</remarks>
public class RotateTransition : TransitionBase<double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the RotateTransition class with the default rotation angle set to zero.
    /// </summary>
    public RotateTransition()
    {
        To = 0;
    }

    /// <summary>
    /// Generates an enumerable collection containing a double animation targeting the angle of a rotate transform
    /// within the associated object's render transform group.
    /// </summary>
    /// <remarks>The generated animation is configured to target the angle property of the first
    /// RotateTransform found within the AssociatedObject's RenderTransform, if it is a TransformGroup. This method
    /// yields no animations if the required transform structure is not present.</remarks>
    /// <returns>An enumerable collection containing a single DoubleAnimation that targets the angle property of a
    /// RotateTransform in the associated object's TransformGroup. If no suitable RotateTransform is found, the
    /// collection is empty.</returns>
    protected override IEnumerable<DoubleAnimation> AnimationGenerate()
    {
        const string Path = "(UIElement.RenderTransform).(TransformGroup.Children)[{0}].(RotateTransform.Angle)";

        if (AssociatedObject.RenderTransform is TransformGroup transformGroup && transformGroup.TryIndexOf<RotateTransform>(out var index))
        {
            DoubleAnimation animation = new DoubleAnimation();

            Storyboard.SetTarget(animation, AssociatedObject);

            Storyboard.SetTargetProperty(animation, new PropertyPath(string.Format(Path, index)));

            yield return animation;
        }
    }
}
