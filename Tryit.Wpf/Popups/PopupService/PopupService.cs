using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using Tryit.Wpf.Services.PopupService;
using static Tryit.Wpf.PopupContext;

namespace Tryit.Wpf;

/// <summary>
/// PopupService manages popups, allowing for displaying messages and confirmations in a UI. It provides methods to set
/// and get popup templates.
/// </summary>
public class PopupService : IPopupService
{
    /// <summary>
    /// A static, read-only dictionary that stores PopupHosted instances associated with string keys. It is thread-safe
    /// for concurrent access.
    /// </summary>
    [DebuggerBrowsable(Never)]
    private static readonly ConcurrentDictionary<string, PopupHosted> hostedStorages = new();

    /// <summary>
    /// A static instance of EasyEvent for handling PubEventArgs events. It is marked to never be browsed in the
    /// debugger.
    /// </summary>
    [DebuggerBrowsable(Never)]
    internal static EasyEvent<PubEventArgs> eventService = new();

    /// <summary>
    /// get show/confirm popup template
    /// example template
    /// <code>
    /// &lt;UserControl&gt;
    ///    &lt;Grid&gt;
    ///        &lt;TextBlock Text="{binding Title}"&gt;
    ///        &lt;TextBlock Text="{binding Content}"&gt;
    ///        &lt;ItemsControl ItemsSource="{binding Buttons}"&gt;
    ///            &lt;ItemsControl.ItemTemplate&gt;
    ///                &lt;DataTemplate&gt;
    ///                    &lt;Button
    ///                        Command="{Binding DataContext.ClickCommand, RelativeSource={RelativeSource Mode=FindAncestor, AncestorLevel=1, AncestorType=ItemsControl}}"
    ///                        CommandParameter="{Binding}"
    ///                        Content="{Binding}"/&gt;
    ///                &lt;/DataTemplate &gt;
    ///            &lt;/ItemsControl.ItemTemplate&gt;
    ///        &lt;/ItemsControl&gt;
    ///    &lt;/Grid&gt;
    /// &lt;/UserControl&gt;
    /// </code>
    /// </summary>
    /// <param name="adornerDecorator"></param>
    /// <returns></returns>
    public static DataTemplate GetPopupTemplate(AdornerDecorator adornerDecorator)
    {
        return (DataTemplate)adornerDecorator.GetValue(PopupTemplateProperty);
    }

    /// <summary>
    /// set show/confirm popup template
    /// example template
    /// <code>
    /// &lt;UserControl&gt;
    ///    &lt;Grid&gt;
    ///        &lt;TextBlock Text="{binding Title}"&gt;
    ///        &lt;TextBlock Text="{binding Content}"&gt;
    ///        &lt;ItemsControl ItemsSource="{binding Buttons}"&gt;
    ///            &lt;ItemsControl.ItemTemplate&gt;
    ///                &lt;DataTemplate&gt;
    ///                    &lt;Button
    ///                        Command="{Binding DataContext.ClickCommand, RelativeSource={RelativeSource Mode=FindAncestor, AncestorLevel=1, AncestorType=ItemsControl}}"
    ///                        CommandParameter="{Binding}"
    ///                        Content="{Binding}"/&gt;
    ///                &lt;/DataTemplate &gt;
    ///            &lt;/ItemsControl.ItemTemplate&gt;
    ///        &lt;/ItemsControl&gt;
    ///    &lt;/Grid&gt;
    /// &lt;/UserControl&gt;
    /// </code>
    /// </summary>
    /// <param name="adornerDecorator"></param>
    /// <param name="value"></param>
    public static void SetPopupTemplate(AdornerDecorator adornerDecorator, DataTemplate value)
    {
        adornerDecorator.SetValue(PopupTemplateProperty, value);
    }

    /// <summary>
    /// show/confirm popup template
    /// example template
    /// <code>
    /// &lt;UserControl&gt;
    ///    &lt;Grid&gt;
    ///        &lt;TextBlock Text="{binding Title}"&gt;
    ///        &lt;TextBlock Text="{binding Content}"&gt;
    ///        &lt;ItemsControl ItemsSource="{binding Buttons}"&gt;
    ///            &lt;ItemsControl.ItemTemplate&gt;
    ///                &lt;DataTemplate&gt;
    ///                    &lt;Button
    ///                        Command="{Binding DataContext.ClickCommand, RelativeSource={RelativeSource Mode=FindAncestor, AncestorLevel=1, AncestorType=ItemsControl}}"
    ///                        CommandParameter="{Binding}"
    ///                        Content="{Binding}"/&gt;
    ///                &lt;/DataTemplate &gt;
    ///            &lt;/ItemsControl.ItemTemplate&gt;
    ///        &lt;/ItemsControl&gt;
    ///    &lt;/Grid&gt;
    /// &lt;/UserControl&gt;
    /// </code>
    /// </summary>
    public static readonly DependencyProperty PopupTemplateProperty = DependencyProperty.RegisterAttached("PopupTemplate", typeof(DataTemplate), typeof(PopupService), new PropertyMetadata(null));

    /// <summary>
    /// Retrieves the hosted name from the specified adorner decorator.
    /// </summary>
    /// <param name="adornerDecorator">The input object from which the hosted name is extracted.</param>
    /// <returns>Returns the hosted name as a string.</returns>
    public static string GetHostedName(AdornerDecorator adornerDecorator)
    {
        return (string)adornerDecorator.GetValue(HostedNameProperty);
    }

    /// <summary>
    /// Sets the hosted name for a specified adorner decorator.
    /// </summary>
    /// <param name="adornerDecorator">The adorner decorator to which the hosted name will be applied.</param>
    /// <param name="value">The name to be set for the hosted element.</param>
    public static void SetHostedName(AdornerDecorator adornerDecorator, string value)
    {
        adornerDecorator.SetValue(HostedNameProperty, value);
    }

    /// <summary>
    /// Defines an attached property for a string named 'HostedName' in PopupService. It manages hosted storages based on
    /// changes to the property.
    /// </summary>
    public static readonly DependencyProperty HostedNameProperty = DependencyProperty.RegisterAttached(
        "HostedName",
        typeof(string),
        typeof(PopupService),
        new PropertyMetadata(
            null,
            static (s, e) =>
            {
                if (s is not AdornerDecorator adornerDecorator)
                {
                    return;
                }

                if (e.OldValue is string oldHostedName)
                {
                    _ = hostedStorages.TryRemove(oldHostedName, out _);
                }

                if (e.NewValue is not string hostedName)
                {
                    return;
                }
                WeakReference weak = new(adornerDecorator);

                hostedStorages[hostedName] = new PopupHosted(weak);
            }
        )
    );

    /// <summary>
    /// Checks if the specified adorner decorator is the main hosted element.
    /// </summary>
    /// <param name="adornerDecorator">The element to check for its main hosted status.</param>
    /// <returns>Returns true if the element is the main hosted element, otherwise false.</returns>
    public static bool GetIsMainHosted(AdornerDecorator adornerDecorator)
    {
        return (bool)adornerDecorator.GetValue(IsMainHostedProperty);
    }

    /// <summary>
    /// Sets the main hosted state of the specified adorner decorator.
    /// </summary>
    /// <param name="adornerDecorator">The visual element that will be modified to reflect the main hosted state.</param>
    /// <param name="value">Indicates whether the adorner decorator should be set as the main hosted element.</param>
    public static void SetIsMainHosted(AdornerDecorator adornerDecorator, bool value)
    {
        adornerDecorator.SetValue(IsMainHostedProperty, value);
    }

    /// <summary>
    /// Defines an attached property named IsMainHosted of type bool for the PopupService class. It is registered as a
    /// DependencyProperty.
    /// </summary>
    public static readonly DependencyProperty IsMainHostedProperty = DependencyProperty.RegisterAttached("IsMainHosted", typeof(bool), typeof(PopupService), new PropertyMetadata(null));

    #region interface

    /// <summary>
    /// show message in main popup host.
    /// Displays a popup asynchronously with specified content and an optional title and configuration.
    /// </summary>
    /// <param name="content">The main text or message to be displayed in the popup.</param>
    /// <param name="title">An optional heading for the popup, defaulting to a predefined title if not provided.</param>
    /// <param name="config">An optional configuration object that can customize the popup's behavior and appearance.</param>
    /// <returns>This method does not return a value; it performs an asynchronous operation to show the popup.</returns>
    public async Task ShowAsync(string content, string? title = null, PopupContext? config = null)
    {
        PopupHosted mainHost = GetMainHost(null!, true);

        PopupContext cfg = config ?? PopupContext.GetDefault(1);

        cfg.Content = content;
        cfg.Title = title ?? cfg.Title ?? "Inotification";

        _ = await mainHost.DisplayAsync(cfg);
    }

    /// <summary>
    /// confirm message in main popup host.
    /// Displays a confirmation popup with specified content and title, returning the user's response.
    /// </summary>
    /// <param name="content">The message displayed in the confirmation popup.</param>
    /// <param name="title">The title of the confirmation popup, defaulting to a standard title if not provided.</param>
    /// <param name="config">Configuration settings for the popup, allowing customization of its behavior.</param>
    /// <returns>The result of the user's interaction with the confirmation popup.</returns>
    public async Task<ButtonResult> ConfirmAsync(string content, string? title = null, PopupContext? config = null)
    {
        var mainHost = GetMainHost(null!, true);

        PopupContext cfg = config ?? PopupContext.GetDefault(3);

        cfg.Content = content;
        cfg.Title = title ?? cfg.Title ?? "Inotification";

        var buttonResult = await mainHost.DisplayAsync(cfg);

        return buttonResult;
    }

    /// <summary>
    /// show message in <paramref name="hostedName"/> popup host.
    /// Displays a notification asynchronously in a specified host with optional title and configuration settings.
    /// </summary>
    /// <param name="hostedName">Specifies the name of the host where the notification will be displayed.</param>
    /// <param name="content">Contains the main message or information to be shown in the notification.</param>
    /// <param name="title">Provides an optional title for the notification, defaulting to a preset value if not provided.</param>
    /// <param name="config">Offers optional configuration settings for the notification's appearance and behavior.</param>
    /// <returns>Returns a task representing the asynchronous operation of displaying the notification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the host name is null, indicating a required parameter is missing.</exception>
    public async Task ShowAsyncIn(string hostedName, string content, string? title = null, PopupContext? config = null)
    {
        _ = hostedName ?? throw new ArgumentNullException(nameof(hostedName));

        var mainHost = GetMainHost(hostedName!, false);

        PopupContext cfg = config ?? PopupContext.GetDefault(1);

        cfg.Content = content;
        cfg.Title = title ?? cfg.Title ?? "Inotification";

        var buttonResult = await mainHost.DisplayAsync(cfg);
    }

    /// <summary>
    /// confirm message in <paramref name="hostedName"/> popup host.
    /// Displays a confirmation popup with specified content and title, returning the user's response.
    /// </summary>
    /// <param name="hostedName">Specifies the host where the confirmation popup will be displayed.</param>
    /// <param name="content">Contains the message or information to be shown in the confirmation popup.</param>
    /// <param name="title">Provides an optional title for the confirmation popup, defaulting to a standard title if not specified.</param>
    /// <param name="config">Allows customization of the popup's behavior and appearance through configuration settings.</param>
    /// <returns>Returns the result of the user's interaction with the confirmation popup.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the host name is null, indicating a required parameter is missing.</exception>
    public async Task<ButtonResult> ConfirmAsyncIn(string hostedName, string content, string? title = null, PopupContext? config = null)
    {
        _ = hostedName ?? throw new ArgumentNullException(nameof(hostedName));

        var mainHost = GetMainHost(hostedName!, false);

        PopupContext cfg = config ?? PopupContext.GetDefault(1);

        cfg.Content = content;
        cfg.Title = title ?? cfg.Title ?? "Inotification";

        var buttonResult = await mainHost.DisplayAsync(cfg);

        return buttonResult;
    }

    /// <summary>
    /// popup visual in main popup host.
    /// Displays a popup asynchronously and returns the result of the operation.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned from the popup operation.</typeparam>
    /// <param name="visual">Specifies the visual element to be displayed in the popup.</param>
    /// <param name="parameter">Provides additional options for configuring the popup behavior.</param>
    /// <returns>The result of the popup operation, which is of the specified type.</returns>
    public async Task<T> PopupAsync<T>(Visual visual, PopupParameter? parameter = null)
    {
        var mainHost = GetMainHost(null!, true);

        var popupResult = await mainHost.PopupAsync<T>(visual, parameter);

        return popupResult;
    }

    /// <summary>
    /// popup visual in <paramref name="hostedName"/> popup host.
    /// Displays a popup asynchronously in a specified host environment and returns the result of the popup operation.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned from the popup operation.</typeparam>
    /// <param name="hostedName">Specifies the name of the host where the popup will be displayed.</param>
    /// <param name="visual">Defines the visual content to be shown in the popup.</param>
    /// <param name="parameter">Contains optional parameters that can modify the behavior of the popup.</param>
    /// <returns>Returns the result of the popup operation as a value of the specified type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the host name provided is null.</exception>
    public async Task<T> PopupAsyncIn<T>(string hostedName, Visual visual, PopupParameter? parameter = null)
    {
        _ = hostedName ?? throw new ArgumentNullException(nameof(hostedName));

        var mainHost = GetMainHost(hostedName!, false);

        var popupResult = await mainHost.PopupAsync<T>(visual, parameter);

        return popupResult;
    }

    /// <summary>
    /// Retrieves the main host based on the provided hosted name and status. It searches through stored hosted items to
    /// find a match.
    /// </summary>
    /// <param name="targetHostedName">Specifies the name of the popup host to locate when not in a hosted state.</param>
    /// <param name="isHosted">Indicates whether to search for the main hosted item or a specific popup host.</param>
    /// <returns>Returns the found PopupHosted instance that matches the criteria.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no matching popup host is found for the specified conditions.</exception>
    private PopupHosted GetMainHost(string targetHostedName, bool isHosted)
    {
        foreach (KeyValuePair<string, PopupHosted> item in hostedStorages)
        {
            if (item.Value.Reference.Target is AdornerDecorator decorator)
            {
                if (isHosted && GetIsMainHosted(decorator))
                {
                    return item.Value;
                }

                if (isHosted == false && GetHostedName(decorator) == targetHostedName)
                {
                    return item.Value;
                }
            }
        }
        var popupIdentity = isHosted ? "main popup host" : $"popup host : {targetHostedName}";
        throw new InvalidOperationException($"{popupIdentity} not configured");
    }

    #endregion


    #region private

    /// <summary>
    /// Represents event data for a publication event, encapsulating relevant information.
    /// </summary>
    /// <param name="content">Holds the message or information related to the publication event.</param>
    /// <param name="Context">Contains the contextual details surrounding the publication event.</param>
    internal record PubEventArgs(string content, PopupContext Context);

    /// <summary>
    /// Handles the display of popups asynchronously, managing user interactions and results.
    /// </summary>
    /// <param name="Reference">Holds a weak reference to the popup host, allowing for safe access without preventing garbage collection.</param>
    private record PopupHosted(WeakReference Reference)
    {
        /// <summary>
        /// A SemaphoreSlim instance is created with an initial count of 1 and a maximum count of 1. It is used to
        /// manage concurrent access to a resource.
        /// </summary>
        private readonly SemaphoreSlim semaphore = new(1, 1);

        /// <summary>
        /// Displays a popup asynchronously and returns the result of the button clicked by the user.
        /// </summary>
        /// <param name="context">Provides the context for the popup, including data and button items for user interaction.</param>
        /// <returns>Returns the result associated with the button that was clicked.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the popup host has expired or if the button clicked is not found in the context.</exception>
        public async Task<ButtonResult> DisplayAsync(PopupContext context)
        {
            try
            {
                await semaphore.WaitAsync();

                if (Reference.Target is not AdornerDecorator decorator)
                {
                    throw new InvalidOperationException("popup host has expired");
                }

                UIElement uielement = default!;

                DataTemplate? datatemplate = GetPopupTemplate(decorator);

                if (datatemplate is not null)
                {
                    uielement = new ContentControl()
                    {
                        DataContext = context,
                        ContentTemplate = datatemplate,
                        Content = context,
                    };
                }
                else
                {
                    uielement = new PopupContainer() { DataContext = context };
                }

                AdornerLayer layer = AdornerLayer.GetAdornerLayer(decorator);

                using ContentAdorner contentAdorner = new(uielement, decorator);

                try
                {
                    layer.Add(contentAdorner);

                    TaskCompletionSource<string> taskCompletion = new();

                    using (
                        eventService.Subscribe(i =>
                        {
                            if (i.Context == context)
                            {
                                taskCompletion.SetResult(i.content);
                            }
                        })
                    )
                    {
                        var buttonContent = await taskCompletion.Task;

                        if (context.buttonItems.Find(i => i.ButtonContent == buttonContent) is ButtonItem buttonItem)
                        {
                            buttonItem.ClickAction?.Invoke(buttonItem.ButtonResult);

                            return buttonItem.ButtonResult;
                        }

                        throw new InvalidOperationException();
                    }
                }
                finally
                {
                    layer.Remove(contentAdorner);
                }
            }
            finally
            {
                _ = semaphore.Release(1);
            }
        }

        /// <summary>
        /// Displays a popup asynchronously and returns a result of a specified type.
        /// </summary>
        /// <typeparam name="T">Specifies the type of the result expected from the popup operation.</typeparam>
        /// <param name="visual">Represents the visual element that will host the popup.</param>
        /// <param name="parameter">Contains optional parameters for configuring the popup behavior.</param>
        /// <returns>Returns the result of the popup operation as the specified type.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the popup host is no longer valid.</exception>
        /// <exception cref="InvalidCastException">Thrown when the result from the popup cannot be cast to the expected type.</exception>
        public async Task<T> PopupAsync<T>(Visual visual, PopupParameter? parameter = null)
        {
            try
            {
                await semaphore.WaitAsync();

                if (Reference.Target is not AdornerDecorator decorator)
                {
                    throw new InvalidOperationException("popup host has expired");
                }

                AdornerLayer layer = AdornerLayer.GetAdornerLayer(decorator);
                using ContentAdorner contentAdorner = new(visual, decorator);

                try
                {
                    layer.Add(contentAdorner);

                    TaskCompletionSource<object> taskCompletion = new();

                    if (GetPopupAware(visual) is IPopupAware popupAware)
                    {
                        popupAware.Opened(parameter);

                        popupAware.RequestCloseEvent += PopupAware_RequestCloseEvent;

                        void PopupAware_RequestCloseEvent(object obj)
                        {
                            popupAware.RequestCloseEvent -= PopupAware_RequestCloseEvent;

                            popupAware.Closed();

                            taskCompletion.SetResult(obj);
                        }
                    }

                    var popupResult = await taskCompletion.Task;

                    if (popupResult is T target)
                    {
                        return target;
                    }

                    throw new InvalidCastException("invalid popup result type");
                }
                finally
                {
                    layer.Remove(contentAdorner);
                }
            }
            finally
            {
                _ = semaphore.Release(1);
            }

            static IPopupAware GetPopupAware(Visual visual)
            {
                if (visual is IPopupAware popupAware)
                {
                    return popupAware;
                }

                if (visual is FrameworkElement frameworkElement && frameworkElement.DataContext is IPopupAware aware)
                {
                    return aware;
                }
                return null!;
            }
        }
    }

    #endregion
}
