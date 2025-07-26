using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Provides extension methods for displaying popups and confirmation dialogs with customizable content, titles, and
/// button labels. Supports both main and hosted popup displays.
/// </summary>
public static class PopupServiceExtensions
{
    /// <summary>
    /// Displays a popup with specified content and an optional title. It can also include custom button labels.
    /// </summary>
    /// <param name="popupService">An interface for displaying popups in the application.</param>
    /// <param name="content">The main message or information to be shown in the popup.</param>
    /// <param name="title">An optional title that appears at the top of the popup.</param>
    /// <param name="buttonContents">An optional dictionary that defines custom labels for the buttons in the popup.</param>
    /// <returns>A task representing the asynchronous operation of showing the popup.</returns>
    public static async Task ShowAsync(this IPopupService popupService, string content, string? title = null, IDictionary<ButtonResult, string>? buttonContents = null)
    {
        PopupContext? context = buttonContents?.ToDictionary(i => i.Key, i => i.Value);
        await popupService.ShowAsync(content, title, context);
    }

    /// <summary>
    /// Displays a confirmation dialog with specified content and optional title and button labels.
    /// </summary>
    /// <param name="popupService">An interface for displaying popup dialogs and handling user interactions.</param>
    /// <param name="content">The message displayed in the confirmation dialog.</param>
    /// <param name="title">An optional title for the confirmation dialog.</param>
    /// <param name="buttonContents">An optional dictionary defining the labels for the buttons in the dialog.</param>
    /// <returns>Returns the result of the user's choice from the confirmation dialog.</returns>
    public static async Task<ButtonResult> ConfirmAsync(this IPopupService popupService, string content, string? title = null, IDictionary<ButtonResult, string>? buttonContents = null)
    {
        PopupContext? context = buttonContents?.ToDictionary(i => i.Key, i => i.Value);
        var result = await popupService.ConfirmAsync(content, title, context);

        return result;
    }

    /// <summary>
    /// show async in <paramref name="hostedName"/>.
    /// Displays a popup asynchronously within a specified host using provided content and optional title and button
    /// configurations.
    /// </summary>
    /// <param name="popupService">An interface for managing popup displays and interactions.</param>
    /// <param name="hostedName">Specifies the name of the host where the popup will be shown.</param>
    /// <param name="content">Contains the main content to be displayed in the popup.</param>
    /// <param name="title">An optional title for the popup to provide context or a heading.</param>
    /// <param name="buttonContents">An optional dictionary defining the text for buttons in the popup.</param>
    /// <returns>Returns a task representing the asynchronous operation of showing the popup.</returns>
    public static async Task ShowAsyncIn(this IPopupService popupService, string hostedName, string content, string? title = null, IDictionary<ButtonResult, string>? buttonContents = null)
    {
        PopupContext? context = buttonContents?.ToDictionary(i => i.Key, i => i.Value);
        await popupService.ShowAsyncIn(hostedName, content, title, context);
    }

    /// <summary>
    /// show async in <paramref name="hostedName"/>.
    /// Displays a confirmation dialog with customizable options and returns the user's selection.
    /// </summary>
    /// <param name="popupService">Facilitates the display and management of popup dialogs.</param>
    /// <param name="content">Specifies the message to be displayed in the confirmation dialog.</param>
    /// <param name="hostedName">Indicates the name of the host where the dialog will be displayed.</param>
    /// <param name="title">Provides an optional title for the confirmation dialog.</param>
    /// <param name="buttonContents">Allows customization of the button labels in the dialog.</param>
    /// <returns>Returns the result of the user's selection from the confirmation dialog.</returns>
    public static async Task<ButtonResult> ConfirmAsyncIn(this IPopupService popupService, string content, string hostedName, string? title = null, IDictionary<ButtonResult, string>? buttonContents = null)
    {
        PopupContext? context = buttonContents?.ToDictionary(i => i.Key, i => i.Value);
        var result = await popupService.ConfirmAsyncIn(hostedName, content, title, context);

        return result;
    }

    /// <summary>
    /// popup visual in main popup host.
    /// Displays a popup asynchronously using the provided visual and optional parameters.
    /// </summary>
    /// <param name="popupService">Facilitates the display and management of popups in the application.</param>
    /// <param name="visual">Defines the visual content to be shown in the popup.</param>
    /// <param name="parameter">Allows for additional configuration options for the popup display.</param>
    /// <returns>Returns the result of the popup operation.</returns>
    public static async Task<object> PopupAsync(this IPopupService popupService, Visual visual, PopupParameter? parameter = null)
    {
        var popupResult = await popupService.PopupAsync<object>(visual, parameter);
        return popupResult;
    }

    /// <summary>
    /// popup visual in <paramref name="hostedName"/> popup host.
    /// Asynchronously displays a popup using the specified service and parameters.
    /// </summary>
    /// <param name="popupService">Used to invoke the popup display functionality.</param>
    /// <param name="hostedName">Specifies the name of the hosted popup to be displayed.</param>
    /// <param name="visual">Defines the visual representation of the popup.</param>
    /// <param name="parameter">Allows for optional additional settings for the popup.</param>
    /// <returns>Returns the result of the popup display operation.</returns>
    public static async Task<object> PopupAsyncIn(this IPopupService popupService, string hostedName, Visual visual, PopupParameter? parameter = null)
    {
        var popupResult = await popupService.PopupAsyncIn<object>(hostedName, visual, parameter);
        return popupResult;
    }

    /// <summary>
    /// <para>popup visual in main popup host </para>
    /// <para>Use <see cref="IViewLocator"/> to locate the view</para>
    /// <para>Before using this method, please first set the <see cref="IViewLocator"/> using method <see cref="ViewLocator.SetViewLocator(IViewLocator)"/></para>
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned from the popup operation.</typeparam>
    /// <param name="popupService">Used to invoke the popup functionality with the specified visual and parameters.</param>
    /// <param name="visualToken">Identifies the visual element to be displayed in the popup.</param>
    /// <param name="parameter">Contains optional settings or data to customize the popup's behavior.</param>
    /// <returns>Returns a task that represents the asynchronous operation, yielding the result of type T.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not properly initialized.</exception>
    public static async Task<T> PopupAsync<T>(this IPopupService popupService, string visualToken, PopupParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        return await popupService.PopupAsync<T>(visual, parameter);
    }

    /// <summary>
    /// <para>popup visual in <paramref name="hostedName"/> popup host </para>
    /// <para>Use <see cref="IViewLocator"/> to locate the view</para>
    /// <para>Before using this method, please first set the <see cref="IViewLocator"/> using method <see cref="ViewLocator.SetViewLocator(IViewLocator)"/></para>
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned from the popup operation.</typeparam>
    /// <param name="popupService">Used to invoke the popup functionality within the application.</param>
    /// <param name="hostedName">Specifies the name of the host where the popup will be displayed.</param>
    /// <param name="visualToken">Identifies the visual element to be used for the popup.</param>
    /// <param name="parameter">Contains additional parameters that may configure the popup's behavior.</param>
    /// <returns>Returns a task that represents the asynchronous operation, yielding the result of type T.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not properly initialized.</exception>
    public static async Task<T> PopupAsyncIn<T>(this IPopupService popupService, string hostedName, string visualToken, PopupParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        return await popupService.PopupAsyncIn<T>(hostedName, visual, parameter);
    }

    /// <summary>
    /// <para>popup visual in main popup host </para>
    /// <para>Use <see cref="IViewLocator"/> to locate the view</para>
    /// <para>Before using this method, please first set the <see cref="IViewLocator"/> using method <see cref="ViewLocator.SetViewLocator(IViewLocator)"/></para>
    /// </summary>
    /// <param name="popupService">Used to invoke the popup display functionality.</param>
    /// <param name="visualToken">Identifies the visual representation to be displayed in the popup.</param>
    /// <param name="parameter">Provides additional options for configuring the popup's behavior.</param>
    /// <returns>Returns an object representing the result of the popup operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not properly initialized.</exception>
    public static async Task<object> PopupAsync(this IPopupService popupService, string visualToken, PopupParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        return await popupService.PopupAsync<object>(visual, parameter);
    }

    /// <summary>
    /// <para>popup visual in <paramref name="hostedName"/> popup host </para>
    /// <para>Use <see cref="IViewLocator"/> to locate the view</para>
    /// <para>Before using this method, please first set the <see cref="IViewLocator"/> using method <see cref="ViewLocator.SetViewLocator(IViewLocator)"/></para>
    /// </summary>
    /// <param name="popupService">Provides the service responsible for displaying the popup.</param>
    /// <param name="hostedName">Specifies the name of the host where the popup will be displayed.</param>
    /// <param name="visualToken">Identifies the visual element to be used for the popup.</param>
    /// <param name="parameter">Contains additional parameters for configuring the popup's behavior.</param>
    /// <returns>Returns an object representing the result of the popup operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the visual locator is not properly initialized.</exception>
    public static async Task<object> PopupAsyncIn(this IPopupService popupService, string hostedName, string visualToken, PopupParameter? parameter = null)
    {
        _ = ViewLocator.viewLocator ?? throw new InvalidOperationException("invalid visual locator");

        var visual = ViewLocator.viewLocator.Locate(visualToken);

        return await popupService.PopupAsyncIn<object>(hostedName, visual, parameter);
    }
}
