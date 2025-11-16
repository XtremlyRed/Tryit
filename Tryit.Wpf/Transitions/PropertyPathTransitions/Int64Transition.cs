using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a nullable 64-bit integer property using an Int64Animation.
/// </summary>
/// <remarks>Use this class to define animations for properties of type Int64? (nullable long) within a property
/// transition system. The transition applies an Int64Animation to interpolate between values over time.</remarks>
public class Int64Transition : PropertyTransitionBase<Int64?, Int64Animation>
{
    /// <summary>
    /// Initializes a new instance of the Int64Transition class with the target value set to zero.
    /// </summary>
    public Int64Transition()
    {
        To = 0;
    }
}
