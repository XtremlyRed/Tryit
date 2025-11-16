using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Provides a transition that animates the opacity mask color of a UI element using a fade effect.
/// </summary>
/// <remarks>The FadeTransition class targets the (UIElement.OpacityMask).(SolidColorBrush.Color) property of the
/// associated UI element. By default, the transition fades to black unless the To property is set to a different color.
/// This class is typically used to create fade-in or fade-out visual effects on UI elements by animating their opacity
/// mask color.</remarks>
public class FadeTransition : TransitionBase<Color?, ColorAnimation>
{
    /// <summary>
    /// Initializes a new instance of the FadeTransition class with default settings.
    /// </summary>
    /// <remarks>The To property is initialized to Colors.Black by default. This sets the target color for the
    /// fade transition unless otherwise specified.</remarks>
    public FadeTransition()
    {
        To = Colors.Black;
    }

    /// <summary>
    /// Generates a sequence of color animations that target the opacity mask color of the associated UI element.
    /// </summary>
    /// <remarks>The returned animation targets the (UIElement.OpacityMask).(SolidColorBrush.Color) property
    /// of the associated object. If the associated object's OpacityMask is not set, it is initialized to a solid black
    /// brush before the animation is applied.</remarks>
    /// <returns>An enumerable collection containing the color animations to be applied to the associated object's opacity mask.</returns>
    protected override IEnumerable<ColorAnimation> AnimationGenerate()
    {
        const string Path = "(UIElement.OpacityMask).(SolidColorBrush.Color)";

        ColorAnimation animation = new ColorAnimation();

        Storyboard.SetTargetProperty(animation, new PropertyPath(Path));

        Storyboard.SetTarget(animation, AssociatedObject);

        base.AssociatedObject.OpacityMask ??= new SolidColorBrush(Colors.Black);

        yield return animation;
    }
}
