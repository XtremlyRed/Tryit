using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a nullable 32-bit integer property using an integer animation.
/// </summary>
public class Int32Transition : PropertyTransitionBase<Int32?, Int32Animation>
{
    /// <summary>
    /// Initializes a new instance of the Int32Transition class with the target value set to 0.
    /// </summary>
    public Int32Transition()
    {
        To = 0;
    }
}
