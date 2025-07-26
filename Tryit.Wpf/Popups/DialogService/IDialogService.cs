using System.Diagnostics;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Interface for dialog services that displays visual elements with optional customization. Includes methods for
/// showing dialogs and returning success status.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Displays a visual element, optionally using additional dialog parameters for customization.
    /// </summary>
    /// <param name="visual">Represents the visual content to be shown in the dialog.</param>
    /// <param name="parameter">Allows for optional customization of the dialog's behavior or appearance.</param>
    void Show(Visual visual, DialogParameter? parameter = null);

    /// <summary>
    /// Displays a dialog based on the provided visual context and optional parameters.
    /// </summary>
    /// <param name="visual">Specifies the visual element that the dialog will be associated with.</param>
    /// <param name="parameter">Allows for additional settings or data to be passed to the dialog.</param>
    /// <returns>Returns a nullable boolean indicating whether the dialog was shown successfully.</returns>
    bool? ShowDialog(Visual visual, DialogParameter? parameter = null);
}
