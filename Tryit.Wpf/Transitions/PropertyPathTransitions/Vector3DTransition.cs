using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a 3D vector property value over time using a Vector3D animation.
/// </summary>
/// <remarks>Use this class to smoothly interpolate between two Vector3D values in animation scenarios, such as
/// animating positions or directions in 3D space. This transition supports nullable Vector3D values, allowing for
/// optional or unset states.</remarks>
public class Vector3DTransition : PropertyTransitionBase<Vector3D?, Vector3DAnimation>
{
    /// <summary>
    /// Initializes a new instance of the Vector3DTransition class with default values.
    /// </summary>
    public Vector3DTransition()
    {
        To = new Vector3D();
    }
}
