using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static Tryit.Wpf.PopupService;

namespace Tryit.Wpf;

/// <summary>
/// Interface for managing popups, allowing asynchronous display of messages, confirmations, and visual content in
/// specified hosts. Supports customization through optional titles and configuration settings.
/// </summary>
public interface IPopupService
{
    /// <summary>
    /// show message in main popup host.
    /// Displays a popup asynchronously with specified content and an optional title. Additional configuration options
    /// can be provided.
    /// </summary>
    /// <param name="content">The main text or message to be displayed in the popup.</param>
    /// <param name="title">An optional heading or title for the popup, enhancing its context.</param>
    /// <param name="config">Optional settings that customize the behavior and appearance of the popup.</param>
    /// <returns>A task representing the asynchronous operation of showing the popup.</returns>
    Task ShowAsync(string content, string? title = null, PopupContext? config = null);

    /// <summary>
    /// confirm message in main popup host.
    /// Asynchronously displays a confirmation dialog with specified content and an optional title and configuration.
    /// </summary>
    /// <param name="content">The message displayed in the confirmation dialog.</param>
    /// <param name="title">An optional title for the confirmation dialog.</param>
    /// <param name="config">Optional settings that customize the behavior and appearance of the dialog.</param>
    /// <returns>A task that represents the asynchronous operation, yielding the result of the user's action.</returns>
    Task<ButtonResult> ConfirmAsync(string content, string? title = null, PopupContext? config = null);

    /// <summary>
    /// show message in <paramref name="hostedName"/> popup host.
    /// Displays a popup asynchronously with specified content and optional title and configuration settings.
    /// </summary>
    /// <param name="hostedName">Specifies the name of the host where the popup will be displayed.</param>
    /// <param name="content">Contains the main content to be shown in the popup.</param>
    /// <param name="title">Provides an optional title for the popup window.</param>
    /// <param name="config">Offers optional configuration settings for the popup's behavior and appearance.</param>
    /// <returns>Returns a Task indicating the completion of the popup display operation.</returns>
    Task ShowAsyncIn(string hostedName, string content, string? title = null, PopupContext? config = null);

    /// <summary>
    /// confirm message in <paramref name="hostedName"/> popup host.
    /// Asynchronously displays a confirmation dialog with specified content and options. It allows customization of the
    /// dialog's title and configuration.
    /// </summary>
    /// <param name="hostedName">Specifies the name of the host where the confirmation dialog will be displayed.</param>
    /// <param name="content">Contains the message that will be shown in the confirmation dialog.</param>
    /// <param name="title">Provides an optional title for the confirmation dialog to give context to the user.</param>
    /// <param name="config">Offers additional configuration options for the appearance and behavior of the dialog.</param>
    /// <returns>Returns a task that represents the user's response to the confirmation dialog.</returns>
    Task<ButtonResult> ConfirmAsyncIn(string hostedName, string content, string? title = null, PopupContext? config = null);

    /// <summary>
    /// popup visual in main popup host.
    /// Asynchronously displays a popup with the specified visual content. It can also accept optional parameters for
    /// customization.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned after the popup operation completes.</typeparam>
    /// <param name="visual">Defines the visual content to be displayed in the popup.</param>
    /// <param name="parameter">Allows for additional customization options for the popup's behavior.</param>
    /// <returns>Returns a task that represents the asynchronous operation, containing the result of the popup action.</returns>
    Task<T> PopupAsync<T>(Visual visual, PopupParameter? parameter = null);

    /// <summary>
    /// popup visual in <paramref name="hostedName"/> popup host.
    /// Displays a popup asynchronously with specified parameters. The popup is associated with a visual element.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned after the popup operation completes.</typeparam>
    /// <param name="hostedName">Specifies the name of the hosted environment for the popup.</param>
    /// <param name="visual">Indicates the visual element that the popup will be associated with.</param>
    /// <param name="parameter">Contains optional parameters that customize the popup's behavior.</param>
    /// <returns>Provides a value task that completes with the result of the popup operation.</returns>
    Task<T> PopupAsyncIn<T>(string hostedName, Visual visual, PopupParameter? parameter = null);
}
