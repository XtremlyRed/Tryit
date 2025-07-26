namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for displaying dialogs using a visual token and optional parameters. Ensures a valid
/// visual locator is available before showing the dialog.
/// </summary>
public static class IDialogServiceExtensions
{
    /// <summary>
    /// Displays a dialog using a specified visual token and optional parameters. It ensures a valid visual locator is
    /// available before proceeding.
    /// </summary>
    /// <param name="dialogService">Used to invoke the dialog display functionality.</param>
    /// <param name="visualToken">Identifies the specific visual representation to be shown in the dialog.</param>
    /// <param name="parameter">Provides additional settings or data to customize the dialog's behavior.</param>
    /// <returns>Returns a nullable boolean indicating the dialog's result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not properly initialized.</exception>
    public static bool? ShowDialog(this IDialogService dialogService, string visualToken, DialogParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        return dialogService.ShowDialog(visual, parameter);
    }

    /// <summary>
    /// Displays a dialog using a specified visual token and optional parameters. It ensures a valid visual locator is
    /// available before proceeding.
    /// </summary>
    /// <param name="dialogService">Used to display the dialog with the located visual and any provided parameters.</param>
    /// <param name="visualToken">Specifies the identifier for the visual to be displayed in the dialog.</param>
    /// <param name="parameter">Allows for optional parameters to be passed to the dialog for customization.</param>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not set, indicating an invalid state for locating visuals.</exception>
    public static void Show(this IDialogService dialogService, string visualToken, DialogParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        dialogService.Show(visual, parameter);
    }
}
