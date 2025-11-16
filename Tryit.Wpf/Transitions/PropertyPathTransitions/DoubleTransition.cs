using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a double-precision floating-point property value over time.
/// </summary>
/// <remarks>Use this class to create smooth transitions for properties of type <see cref="Double"/>. The
/// transition defines how the property's value changes from its current value to a specified target value using a
/// double animation. This class is typically used in UI frameworks to animate visual properties such as opacity, width,
/// or position.</remarks>
public class DoubleTransition : PropertyTransitionBase<Double?, DoubleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the DoubleTransition class with the default target value.
    /// </summary>
    public DoubleTransition()
    {
        To = 0;
    }
}
