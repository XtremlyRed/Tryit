using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a property of type <see cref="Point"/> using a <see cref="PointAnimation"/>.
/// </summary>
/// <remarks>Use this class to animate properties that hold nullable <see cref="Point"/> values, such as position
/// or coordinates, with customizable animation behavior. This class is typically used in UI frameworks that support
/// property transitions and animations.</remarks>
public class PointTransition : PropertyTransitionBase<Point?, PointAnimation>
{
    /// <summary>
    /// Initializes a new instance of the PointTransition class with default values.
    /// </summary>
    public PointTransition()
    {
        To = new Point();
    }
}
