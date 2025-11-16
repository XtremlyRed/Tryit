using System.Windows;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a property of type Vector using a VectorAnimation.
/// </summary>
public class VectorTransition : PropertyTransitionBase<Vector?, VectorAnimation>
{
    /// <summary>
    /// Initializes a new instance of the VectorTransition class with default values.
    /// </summary>
    public VectorTransition()
    {
        To = new Vector();
    }
}
