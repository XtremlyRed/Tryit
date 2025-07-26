using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

internal class ContentAdorner : System.Windows.Documents.Adorner, IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Visual visual;

    /// <summary>
    /// Initializes a new instance of the ContentAdorner class, which adds a visual element to an adorned UI element.
    /// </summary>
    /// <param name="visual">Represents the visual element that will be added to the adorned UI element.</param>
    /// <param name="adornedElement">Represents the UI element that is being enhanced with additional visual content.</param>
    public ContentAdorner(Visual visual, UIElement adornedElement)
        : base(adornedElement)
    {
        this.visual = visual;

        AddVisualChild(visual);
    }

    protected override int VisualChildrenCount => 1;

    protected override Visual GetVisualChild(int index)
    {
        return visual;
    }

    /// <summary>
    /// Arranges the child elements of a UI element within the specified size.
    /// </summary>
    /// <param name="finalSize">Specifies the size within which the child elements are arranged.</param>
    /// <returns>Returns the size that was used for arranging the child elements.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        (visual as UIElement)?.Arrange(new Rect(finalSize));
        return finalSize;
    }

    /// <summary>
    /// Releases resources used by the visual element. Removes the visual child if it exists and sets the visual
    /// reference to null.
    /// </summary>
    public void Dispose()
    {
        if (visual is not null)
        {
            RemoveVisualChild(visual);
        }

        visual = null!;
    }
}
