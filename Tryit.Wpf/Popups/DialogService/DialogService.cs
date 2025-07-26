using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Manages dialog windows, allowing for their display and interaction. It provides methods to show dialogs and handle
/// their results. Handles initialization and closing events for dialogs.
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>
    /// A SemaphoreSlim instance initialized with a maximum count of 1, allowing one thread to enter the semaphore at a
    /// time.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    SemaphoreSlim semaphoreSlim = new(1, 1);

    /// <summary>
    /// Displays a dialog window using the provided visual element and optional parameters.
    /// </summary>
    /// <param name="visual">The visual element to be displayed in the dialog window.</param>
    /// <param name="parameter">Optional settings or parameters that customize the dialog's behavior.</param>
    /// <exception cref="ArgumentNullException">Thrown when the dialog window or visual element is null.</exception>
    public void Show(Visual visual, DialogParameter? parameter = null)
    {
        _ = dialogWindiw ?? throw new ArgumentNullException(nameof(dialogWindiw));
        _ = visual ?? throw new ArgumentNullException(nameof(visual));

        semaphoreSlim.Wait();

        var dialogWindow = InnerInit(visual, parameter);

        dialogWindow.Show();
    }

    /// <summary>
    /// Displays a dialog window using the provided visual and optional parameters. It returns a nullable boolean indicating
    /// the dialog's result.
    /// </summary>
    /// <param name="visual">Specifies the visual element to be displayed in the dialog.</param>
    /// <param name="parameter">Provides additional settings or options for the dialog, if any.</param>
    /// <returns>Returns a nullable boolean that indicates whether the dialog was accepted or canceled.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the dialog window or visual element is null.</exception>
    public bool? ShowDialog(Visual visual, DialogParameter? parameter = null)
    {
        _ = dialogWindiw ?? throw new ArgumentNullException(nameof(dialogWindiw));
        _ = visual ?? throw new ArgumentNullException(nameof(visual));

        semaphoreSlim.Wait();

        var dialogWindow = InnerInit(visual, parameter);

        return dialogWindow.ShowDialog();
    }

    /// <summary>
    /// Initializes a dialog window with a specified visual and optional parameters, setting up event handlers for
    /// closing events.
    /// </summary>
    /// <param name="visual">Specifies the content to be displayed in the dialog window.</param>
    /// <param name="parameter">Provides optional parameters for the dialog's initialization process.</param>
    /// <returns>Returns the initialized dialog window instance.</returns>
    private Window InnerInit(Visual visual, DialogParameter? parameter = null)
    {
        dialogWindiw!.Content = visual;
        dialogWindiw.Closed += DialogWindiw_Closed;

        if (visual is IDialogAware aware)
        {
            aware.RequestCloseEvent += Aware_RequestCloseEvent;
            aware.Opened(parameter);
        }

        dialogWindiw.Owner = Application.Current.MainWindow;

        return dialogWindiw;

        void DialogWindiw_Closed(object? sender, EventArgs e)
        {
            if (sender is not Window windiw)
            {
                return;
            }

            windiw!.Closed -= DialogWindiw_Closed;

            if (windiw.Content is IDialogAware dialogAware)
            {
                dialogAware.RequestCloseEvent += Aware_RequestCloseEvent;
                dialogAware.Closed();
            }
            else if (windiw.Content is FrameworkElement element && element.DataContext is IDialogAware aware)
            {
                aware.RequestCloseEvent += Aware_RequestCloseEvent;
                aware.Closed();
            }

            semaphoreSlim.Release();
        }

        void Aware_RequestCloseEvent(object obj)
        {
            dialogWindiw?.Close();
        }
    }

    /// <summary>
    /// A static nullable variable that holds a reference to a dialog window. It can be used to manage the state of a
    /// dialog in the application.
    /// </summary>
    static Window? dialogWindiw;

    /// <summary>
    /// Sets the dialog window for the dialog service, allowing for a specific window to be used for dialogs.
    /// </summary>
    /// <param name="dialogWindiw">The parameter specifies the window that will be used for displaying dialogs.</param>
    /// <exception cref="ArgumentNullException">Thrown when the provided window is null.</exception>
    public static void SetDialogWindiw(Window dialogWindiw)
    {
        DialogService.dialogWindiw = dialogWindiw ?? throw new ArgumentNullException(nameof(dialogWindiw));
    }
}

/// <summary>
/// Defines methods and an event for handling dialog lifecycle events. It includes opening, closing, and requesting
/// closure of dialogs.
/// </summary>
public interface IDialogAware
{
    /// <summary>
    /// Handles the event when a dialog is opened, allowing for optional parameters to be passed.
    /// </summary>
    /// <param name="parameter">An optional value that can provide additional context or data when the dialog is opened.</param>
    void Opened(DialogParameter? parameter);

    /// <summary>
    /// Closes the current operation or process. It may trigger cleanup or finalization tasks.
    /// </summary>
    void Closed();

    /// <summary>
    /// An event that is triggered to request the closure of a component. It can carry an optional object parameter.
    /// </summary>
    event Action<object>? RequestCloseEvent;
}
