using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition for animating a nullable 16-bit integer property using an Int16Animation.
/// </summary>
public class Int16Transition : PropertyTransitionBase<Int16?, Int16Animation>
{
    /// <summary>
    /// Initializes a new instance of the Int16Transition class with the target value set to zero.
    /// </summary>
    public Int16Transition()
    {
        To = 0;
    }
}
