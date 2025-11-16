using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a 3D point value over time.
/// </summary>
/// <remarks>Use this class to animate properties of type <see cref="Point3D"/> or nullable <see cref="Point3D"/>
/// in 3D graphics scenarios. The transition defines how the value changes from its current state to a target value
/// using a <see cref="Point3DAnimation"/>.</remarks>
public class Point3DTransition : PropertyTransitionBase<Point3D?, Point3DAnimation>
{
    /// <summary>
    /// Initializes a new instance of the Point3DTransition class with default values.
    /// </summary>
    public Point3DTransition()
    {
        To = new Point3D();
    }
}
