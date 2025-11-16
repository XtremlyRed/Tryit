using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a property of type <see cref="Quaternion?"/> using a <see
/// cref="QuaternionAnimation"/>.
/// </summary>
/// <remarks>Use this class to smoothly interpolate between quaternion values, such as for animating rotations in
/// 3D graphics. The transition can be configured with target values and animation parameters as needed.</remarks>
public class QuaternionTransition : PropertyTransitionBase<Quaternion?, QuaternionAnimation>
{
    /// <summary>
    /// Initializes a new instance of the QuaternionTransition class with the target quaternion set to the identity
    /// value.
    /// </summary>
    public QuaternionTransition()
    {
        To = new Quaternion();
    }
}
