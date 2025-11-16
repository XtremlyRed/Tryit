using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition for a nullable byte value, initializing the transition target to zero.
/// </summary>
public class ByteTransition : PropertyTransitionBase<Byte?, ByteAnimation>
{
    /// <summary>
    /// Initializes a new instance of the ByteTransition class with the transition value set to zero.
    /// </summary>
    public ByteTransition()
    {
        To = 0;
    }
}
