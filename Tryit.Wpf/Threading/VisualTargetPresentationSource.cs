using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Tryit.Wpf.Threading;

/// <summary>
///     The VisualTargetPresentationSource represents the root
///     of a visual subtree owned by a different thread that the
///     visual tree in which is is displayed.
/// </summary>
/// <remarks>
///     A HostVisual belongs to the same UI thread that owns the
///     visual tree in which it resides.
///
///     A HostVisual can reference a VisualTarget owned by another
///     thread.
///
///     A VisualTarget has a root visual.
///
///     VisualTargetPresentationSource wraps the VisualTarget and
///     enables basic functionality like Loaded, which depends on
///     a PresentationSource being available.
/// </remarks>
public class VisualTargetPresentationSource : PresentationSource, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the VisualTargetPresentationSource class using the specified host visual.
    /// </summary>
    /// <param name="hostVisual">The HostVisual that will host the visual target. Cannot be null.</param>
    public VisualTargetPresentationSource(HostVisual hostVisual)
    {
        _visualTarget = new VisualTarget(hostVisual);
    }

    /// <summary>
    /// Gets or sets the root visual object that defines the visual tree for this presentation source.
    /// </summary>
    /// <remarks>Setting this property changes the visual content displayed by the presentation source. The
    /// root visual is used as the entry point for rendering and layout. Assigning a new root visual will detach event
    /// handlers from the previous root, attach them to the new root if applicable, and may trigger layout and data
    /// binding updates. This property cannot be set after the object has been disposed.</remarks>
    public override Visual RootVisual
    {
        get => _visualTarget.RootVisual;
        set
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("VisualTarget");
            }

            Visual oldRoot = _visualTarget.RootVisual;

            // Set the root visual of the VisualTarget.  This visual will
            // now be used to visually compose the scene.
            _visualTarget.RootVisual = value;

            // Hook the SizeChanged event on framework elements for all
            // future changed to the layout size of our root, and manually
            // trigger a size change.
            if (oldRoot is FrameworkElement oldRootFe)
            {
                oldRootFe.SizeChanged -= root_SizeChanged;
            }
            if (value is FrameworkElement rootFe)
            {
                rootFe.SizeChanged += root_SizeChanged;
                rootFe.DataContext = _dataContext;

                if (_propertyName != null)
                {
                    Binding myBinding = new(_propertyName) { Source = _dataContext };
                    rootFe.SetBinding(TextBlock.TextProperty, myBinding);
                }
            }

            // Tell the PresentationSource that the root visual has
            // changed.  This kicks off a bunch of stuff like the
            // Loaded event.
            RootChanged(oldRoot, value);

            // Kickoff layout...
            if (value is UIElement rootElement)
            {
                rootElement.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                rootElement.Arrange(new Rect(rootElement.DesiredSize));
            }
        }
    }

    /// <summary>
    /// Gets or sets the data context for the visual target, which is used to provide data binding for child elements.
    /// </summary>
    /// <remarks>Setting this property updates the data context of the root visual element if it is a
    /// FrameworkElement. This enables data binding scenarios for elements within the visual tree. If the object has
    /// been disposed, setting this property will throw an ObjectDisposedException.</remarks>
    public object DataContext
    {
        get => _dataContext;
        set
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("VisualTarget");
            }

            if (_dataContext == value)
            {
                return;
            }

            _dataContext = value;
            if (_visualTarget.RootVisual is FrameworkElement rootElement)
            {
                rootElement.DataContext = _dataContext;
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the property to bind to the root visual element's text.
    /// </summary>
    /// <remarks>Setting this property updates the binding of the root visual element's text to the specified
    /// property name using the current data context. An exception is thrown if the object has been disposed or if the
    /// calling thread does not have access to the root visual element.</remarks>
    public string PropertyName
    {
        get => _propertyName;
        set
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("VisualTarget");
            }

            _propertyName = value;

            if (_visualTarget.RootVisual is TextBlock rootElement)
            {
                if (!rootElement.CheckAccess())
                {
                    throw new InvalidOperationException("What?");
                }

                Binding myBinding = new(_propertyName) { Source = _dataContext };
                rootElement.SetBinding(TextBlock.TextProperty, myBinding);
            }
        }
    }

    /// <summary>
    /// Occurs when the size of the element changes.
    /// </summary>
    /// <remarks>This event is raised whenever the element's width or height is modified, either
    /// programmatically or as a result of layout changes. Handlers can use the event data to determine the previous and
    /// new size values.</remarks>
    public event SizeChangedEventHandler? SizeChanged;

    /// <summary>
    /// Gets a value indicating whether the object has been disposed.
    /// </summary>
    public override bool IsDisposed => _isDisposed;

    /// <summary>
    /// Provides the CompositionTarget that is used to display the visual content of the host visual.
    /// </summary>
    /// <returns>A CompositionTarget instance that represents the rendering target for the host visual.</returns>
    protected override CompositionTarget GetCompositionTargetCore()
    {
        return _visualTarget;
    }

    /// <summary>
    /// Handles the event that occurs when the size of the root element changes.
    /// </summary>
    /// <param name="sender">The source of the event, typically the root element whose size has changed.</param>
    /// <param name="e">A <see cref="SizeChangedEventArgs"/> that contains the event data related to the size change.</param>
    private void root_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        SizeChanged?.Invoke(this, e);
    }

    private readonly VisualTarget _visualTarget = default!;
    private object _dataContext = default!;
    private string _propertyName = default!;
    private bool _isDisposed;

    /// <summary>
    /// Releases all resources used by the current instance.
    /// </summary>
    /// <remarks>Call this method when the instance is no longer needed to free unmanaged resources promptly.
    /// After calling this method, the instance should not be used.</remarks>
    public void Dispose()
    {
        _visualTarget?.Dispose();
        _isDisposed = true;
    }
}
