using System.Windows;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates changes to a Thickness property value over time.
/// </summary>
/// <remarks>Use this class to smoothly animate layout-related properties, such as margins or padding, that are
/// represented by Thickness. The transition interpolates between the starting and ending Thickness values using a
/// ThicknessAnimation.</remarks>
public class ThicknessTransition : PropertyTransitionBase<Thickness?, ThicknessAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ThicknessTransition class with default values.
    /// </summary>
    public ThicknessTransition()
    {
        To = new Thickness();
    }
}
