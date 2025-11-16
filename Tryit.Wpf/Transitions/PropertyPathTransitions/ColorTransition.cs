using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a color property from one value to another over time.
/// </summary>
/// <remarks>Use this class to animate color changes in UI elements, such as fading from one color to another. The
/// transition uses a color animation to interpolate between the starting and ending color values. By default, the
/// target color is set to black.</remarks>
public class ColorTransition : PropertyTransitionBase<Color?, ColorAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ColorTransition class with default values.
    /// </summary>
    /// <remarks>The To property is initialized to Colors.Black by default. Other properties should be set as
    /// needed after construction.</remarks>
    public ColorTransition()
    {
        To = Colors.Black;
    }
}
