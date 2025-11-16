using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a single-precision floating-point property value over time.
/// </summary>
/// <remarks>Use this class to define animations for properties of type <see cref="Single"/> (float) that require
/// smooth transitions between values. The transition can be customized by setting properties such as the target value
/// and animation parameters. This class is typically used in scenarios where property changes should be visually
/// interpolated rather than applied instantly.</remarks>
public class SingleTransition : PropertyTransitionBase<Single?, SingleAnimation>
{
    /// <summary>
    /// Initializes a new instance of the SingleTransition class with default values.
    /// </summary>
    public SingleTransition()
    {
        To = 0;
    }
}
