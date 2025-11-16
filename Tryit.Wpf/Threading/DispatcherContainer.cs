using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf.Threading;

/// <summary>
/// Hosts a UIElement on a separate dispatcher thread, enabling cross-thread visual composition within a WPF
/// application.
/// </summary>
/// <remarks>DispatcherContainer allows a child UIElement to be created and managed on a dedicated dispatcher,
/// which can be useful for isolating complex visuals or improving UI responsiveness. The container manages the lifetime
/// and presentation of the child element, ensuring proper integration with the WPF visual tree. Thread affinity and
/// dispatcher context should be considered when interacting with the child element.</remarks>
[ContentProperty(nameof(Child))]
public sealed class DispatcherContainer : FrameworkElement
{
    /// <summary>
    /// Initializes a new instance of the DispatcherContainer class.
    /// </summary>
    public DispatcherContainer()
    {
        _hostVisual = new InteractiveHostVisual();
    }

    /// <summary>
    /// Represents the visual host used to display visual content in a separate visual tree.
    /// </summary>
    private readonly HostVisual _hostVisual;

    /// <summary>
    ///
    /// </summary>
    private VisualTargetPresentationSource _targetSource = default!;

    #region Child

    private bool _isUpdatingChild;

    /// <summary>
    /// Gets the child UI element contained within this element.
    /// </summary>
    public UIElement Child { get; private set; } = default!;

    /// <summary>
    /// Asynchronously creates a new instance of the specified UIElement type and sets it as the child element.
    /// </summary>
    /// <typeparam name="T">The type of UIElement to create and set as the child. Must have a parameterless constructor.</typeparam>
    /// <param name="dispatcher">An optional dispatcher to use for UI thread operations. If null, the default dispatcher is used.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetChildAsync<T>(Dispatcher? dispatcher = null)
        where T : UIElement, new()
    {
        await SetChildAsync(() => new T(), dispatcher);
    }

    /// <summary>
    /// Asynchronously creates a new child UI element using the specified factory function and sets it as the current
    /// child, optionally using the provided dispatcher.
    /// </summary>
    /// <typeparam name="T">The type of UI element to create and set as the child. Must derive from UIElement.</typeparam>
    /// <param name="new">A factory function that creates an instance of the child UI element. Cannot be null.</param>
    /// <param name="dispatcher">The dispatcher to use for creating and setting the child element. If null, a new dispatcher is created for the
    /// operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetChildAsync<T>(Func<T> @new, Dispatcher? dispatcher = null)
        where T : UIElement
    {
        dispatcher ??= await UIDispatcher.RunNewAsync($"{typeof(T).Name}");
        T child = await dispatcher.InvokeAsync(@new);
        await SetChildAsync(child);
    }

    /// <summary>
    /// Asynchronously sets the child UI element for the host, replacing any existing child.
    /// </summary>
    /// <remarks>If a child is already present, it is removed before the new child is set. This method must
    /// not be called recursively or while another child update is in progress.</remarks>
    /// <param name="value">The UIElement to set as the new child. Can be null to remove the current child.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the child is being updated when this method is called.</exception>
    public async Task SetChildAsync(UIElement value)
    {
        if (_isUpdatingChild)
        {
            throw new InvalidOperationException("Child property should not be set during Child updating.");
        }

        _isUpdatingChild = true;
        try
        {
            await SetChildAsync();
        }
        finally
        {
            _isUpdatingChild = false;
        }

        async Task SetChildAsync()
        {
            UIElement oldChild = Child;
            VisualTargetPresentationSource visualTarget = _targetSource;

            if (Equals(oldChild, value))
            {
                return;
            }

            _targetSource = null!;
            if (visualTarget != null)
            {
                RemoveVisualChild(oldChild);
                await visualTarget.Dispatcher.InvokeAsync(visualTarget.Dispose);
            }

            Child = value;

            if (value == null)
            {
                _targetSource = null!;
            }
            else
            {
                await value.Dispatcher.InvokeAsync(() =>
                {
                    _targetSource = new VisualTargetPresentationSource(_hostVisual) { RootVisual = value };
                });
                AddVisualChild(_hostVisual);
            }
            InvalidateMeasure();
        }
    }

    #endregion

    #region Tree & Layout

    /// <summary>
    /// Returns the child visual at the specified index.
    /// </summary>
    /// <remarks>This override supports the visual tree infrastructure for elements with a single visual
    /// child. Accessing any index other than 0 is not supported.</remarks>
    /// <param name="index">The zero-based index of the child visual to retrieve. Must be 0, as this element has only one visual child.</param>
    /// <returns>The child visual at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/> is not 0.</exception>
    protected override Visual GetVisualChild(int index)
    {
        return index != 0 ? throw new ArgumentOutOfRangeException(nameof(index)) : (Visual)_hostVisual;
    }

    /// <summary>
    /// Gets the number of visual child elements contained within this element.
    /// </summary>
    /// <remarks>This property returns 1 if the element has a child; otherwise, it returns 0. It is typically
    /// used when overriding visual tree management methods in custom controls.</remarks>
    protected override int VisualChildrenCount => Child != null ? 1 : 0;

    /// <summary>
    /// Measures the size required for the child element of this control, given an available size constraint.
    /// </summary>
    /// <remarks>This method schedules an asynchronous measure pass for the child element but always returns
    /// Size.Empty. As a result, layout behavior may not be as expected. Override this method to provide custom
    /// measuring logic if needed.</remarks>
    /// <param name="availableSize">The maximum size that the child element can occupy. This value may be infinite to indicate that the element can
    /// be as large as it wants.</param>
    /// <returns>A Size structure representing the desired size of the control. Returns Size.Empty if there is no child element.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        UIElement child = Child;
        if (child == null)
        {
            return default(Size);
        }

        child.Dispatcher.InvokeAsync(() => child.Measure(availableSize), DispatcherPriority.Loaded);

        return default(Size);
    }

    /// <summary>
    /// Arranges the content of the decorator and determines its final size.
    /// </summary>
    /// <remarks>This method asynchronously arranges the child element, which may result in layout updates
    /// occurring after the method returns. Override this method to customize the arrangement of the decorator's child
    /// element.</remarks>
    /// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its child.</param>
    /// <returns>The actual size used after arranging the child element.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        UIElement child = Child;
        if (child == null)
        {
            return finalSize;
        }

        child.Dispatcher.InvokeAsync(() => child.Arrange(new Rect(finalSize)), DispatcherPriority.Loaded);

        return finalSize;
    }

    #endregion

    #region HitTest

    /// <summary>
    /// Performs hit testing to determine whether the specified point intersects with the visual content or its child
    /// element.
    /// </summary>
    /// <remarks>This method is typically called by the WPF hit testing infrastructure and is not intended to
    /// be called directly in application code. The method accounts for the margin of the child element when determining
    /// hit test results.</remarks>
    /// <param name="htp">The hit test parameters that specify the point to test against the visual content.</param>
    /// <returns>A HitTestResult that represents the result of the hit test. If the point intersects with the child element, the
    /// corresponding result is returned; otherwise, a result for this element is returned.</returns>
    protected override HitTestResult HitTestCore(PointHitTestParameters htp)
    {
        UIElement child = Child;

        IInputElement? element = child?.Dispatcher.Invoke(
            () =>
            {
                double offsetX = 0d,
                    offsetY = 0d;
                if (child is FrameworkElement fe)
                {
                    offsetX = fe.Margin.Left;
                    offsetY = fe.Margin.Top;
                }
                return Child.InputHitTest(new Point(htp.HitPoint.X - offsetX, htp.HitPoint.Y - offsetY));
            },
            DispatcherPriority.Normal
        );
        return (HitTestResult)element! ?? new PointHitTestResult(this, htp.HitPoint);
    }

    #endregion
}
