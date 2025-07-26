using System.ComponentModel;

namespace Tryit.Wpf;

/// <summary>
/// Interface for handling popup events, including opening and closing operations. It allows passing optional parameters
/// during popup opening.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IPopupAware
{
    /// <summary>
    /// Handles the event when a popup is opened, allowing for optional parameters to be passed.
    /// </summary>
    /// <param name="parameter">An optional object that can provide additional context or data related to the popup being opened.</param>
    void Opened(PopupParameter? parameter);

    /// <summary>
    /// Closes the current operation or process. It typically finalizes any ongoing tasks.
    /// </summary>
    void Closed();

    /// <summary>
    /// An event that is triggered to request the closure of a component. It can carry an optional object parameter.
    /// </summary>
    event Action<object>? RequestCloseEvent;
}
