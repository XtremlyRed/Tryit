using System.Windows;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

/// <summary>
/// Represents a transition that animates a rectangle value over time using a specified animation behavior.
/// </summary>
/// <remarks>Use this class to animate properties of type <see cref="Rect"/> or nullable <see cref="Rect"/>. The
/// transition applies a <see cref="RectAnimation"/> to interpolate between rectangle values during the animation
/// period.</remarks>
public class RectTransition : PropertyTransitionBase<Rect?, RectAnimation>
{
    /// <summary>
    /// Initializes a new instance of the RectTransition class with default values.
    /// </summary>
    public RectTransition()
    {
        To = new Rect();
    }
}
