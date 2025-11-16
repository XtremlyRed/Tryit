using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a nullable decimal property value over time.
/// </summary>
/// <remarks>Use this class to smoothly animate changes to properties of type <see cref="decimal"/> using a <see
/// cref="DecimalAnimation"/>. This is typically used in UI frameworks that support property transitions for visual
/// effects.</remarks>
public class DecimalTransition : PropertyTransitionBase<Decimal?, DecimalAnimation>
{
    /// <summary>
    /// Initializes a new instance of the DecimalTransition class with the target value set to zero.
    /// </summary>
    public DecimalTransition()
    {
        To = 0;
    }
}
