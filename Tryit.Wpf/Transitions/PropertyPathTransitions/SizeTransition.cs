using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates changes to a <see cref="Size"/> property using a <see cref="SizeAnimation"/>.
/// </summary>
/// <remarks>Use this class to smoothly animate changes in size for UI elements that expose a <see cref="Size"/>
/// property. The transition can be customized by configuring the animation parameters inherited from <see
/// cref="PropertyTransitionBase{TProperty, TAnimation}"/>.</remarks>
public class SizeTransition : PropertyTransitionBase<Size?, SizeAnimation>
{
    /// <summary>
    /// Initializes a new instance of the SizeTransition class with default values.
    /// </summary>
    public SizeTransition()
    {
        To = new Size();
    }
}
