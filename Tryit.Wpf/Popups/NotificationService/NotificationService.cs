using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Tryit.Wpf.Services.NotificationService;

namespace Tryit.Wpf;

/// <summary>
/// Handles notification services, allowing asynchronous notifications to be sent and managed for UI elements. Includes
/// methods for setting templates and managing hosted states.
/// </summary>
public class NotificationService : INotificationService
{
    /// <summary>
    /// A static, read-only dictionary that stores instances of NofityHosted, indexed by string keys. It is designed for
    /// concurrent access.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static readonly ConcurrentDictionary<string, NofityHosted> hostedStorages = new();

    /// <summary>
    /// A static instance of EasyEvent that uses PubEventArgs for event handling. It is not visible in the debugger.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static EasyEvent<PubEventArgs> eventService = new();

    /// <summary>
    /// Sends a notification asynchronously with a specified message and duration.
    /// </summary>
    /// <param name="message">The content of the notification to be sent.</param>
    /// <param name="timeSpan">The duration for which the notification should be displayed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task NotifyAsync(string message, TimeSpan timeSpan)
    {
        var mainHost = GetMainHost(null!, true);

        await mainHost.NotifyAsync(message, timeSpan);
    }

    /// <summary>
    /// Sends a notification to a specified host after a defined time period.
    /// </summary>
    /// <param name="hostedName">Specifies the name of the host to which the notification will be sent.</param>
    /// <param name="message">Contains the content of the notification to be delivered.</param>
    /// <param name="timeSpan">Indicates the delay duration before the notification is sent.</param>
    /// <returns>Returns a task representing the asynchronous operation of sending the notification.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the host name provided is null.</exception>
    public async Task NotifyAsyncIn(string hostedName, string message, TimeSpan timeSpan)
    {
        _ = hostedName ?? throw new ArgumentNullException(nameof(hostedName));

        var mainHost = GetMainHost(hostedName!, false);

        await mainHost.NotifyAsync(message, timeSpan);
    }

    /// <summary>
    /// Retrieves a data template associated with a specified dependency object.
    /// </summary>
    /// <param name="obj">The dependency object from which the notification template is retrieved.</param>
    /// <returns>Returns the data template linked to the specified dependency object.</returns>
    public static DataTemplate GetNotificationTemplate(DependencyObject obj)
    {
        return (DataTemplate)obj.GetValue(NotificationTemplateProperty);
    }

    /// <summary>
    /// Sets a notification template for a specified dependency object.
    /// </summary>
    /// <param name="obj">The object to which the notification template will be applied.</param>
    /// <param name="value">The data template that defines the visual representation of the notification.</param>
    public static void SetNotificationTemplate(DependencyObject obj, DataTemplate value)
    {
        obj.SetValue(NotificationTemplateProperty, value);
    }

    /// <summary>
    /// Registers an attached dependency property named 'NotificationTemplate' of type DataTemplate. It allows for the
    /// definition of a template for notifications.
    /// </summary>
    public static readonly DependencyProperty NotificationTemplateProperty = DependencyProperty.RegisterAttached("NotificationTemplate", typeof(DataTemplate), typeof(NotificationService), new PropertyMetadata(null));

    /// <summary>
    /// Determines if the specified adorner decorator is the main hosted element.
    /// </summary>
    /// <param name="adornerDecorator">The element used to check if it is the main hosted element.</param>
    /// <returns>Returns true if the element is the main hosted element, otherwise false.</returns>
    public static bool GetIsMainHosted(AdornerDecorator adornerDecorator)
    {
        return (bool)adornerDecorator.GetValue(IsMainHostedProperty);
    }

    /// <summary>
    /// Sets the main hosted state for a specified adorner decorator.
    /// </summary>
    /// <param name="adornerDecorator">The adorner decorator that will have its main hosted state modified.</param>
    /// <param name="value">Indicates whether the adorner decorator should be set as the main hosted element.</param>
    public static void SetIsMainHosted(AdornerDecorator adornerDecorator, bool value)
    {
        adornerDecorator.SetValue(IsMainHostedProperty, value);
    }

    /// <summary>
    /// Registers an attached dependency property named 'IsMainHosted' of type boolean for the NotificationService class.
    /// It has a default value of false.
    /// </summary>
    public static readonly DependencyProperty IsMainHostedProperty = DependencyProperty.RegisterAttached("IsMainHosted", typeof(bool), typeof(NotificationService), new PropertyMetadata(false));

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
    /// <param name="adornerDecorator">The adorner decorator where the hosted name will be set.</param>
    /// <param name="value">The name to be assigned to the hosted element.</param>
    public static void SetHostedName(AdornerDecorator adornerDecorator, string value)
    {
        adornerDecorator.SetValue(HostedNameProperty, value);
    }

    /// <summary>
    /// Registers an attached property 'HostedName' of type string for NotificationService. It manages hosted storages based
    /// on changes to the property value.
    /// </summary>
    public static readonly DependencyProperty HostedNameProperty = DependencyProperty.RegisterAttached(
        "HostedName",
        typeof(string),
        typeof(NotificationService),
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

                hostedStorages[hostedName] = new NofityHosted(weak);
            }
        )
    );

    /// <summary>
    /// Retrieves the main host based on the specified hosted name and its hosted status.
    /// </summary>
    /// <param name="targetHostedName">Specifies the name of the hosted entity to be retrieved when not hosted.</param>
    /// <param name="isHosted">Indicates whether to retrieve the main host or a specific hosted entity.</param>
    /// <returns>Returns the corresponding NofityHosted instance based on the provided parameters.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the requested host configuration is not found.</exception>
    private static NofityHosted GetMainHost(string targetHostedName, bool isHosted)
    {
        foreach (KeyValuePair<string, NofityHosted> item in hostedStorages)
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
        var popupIdentity = isHosted ? "main notify host" : $"notify host : {targetHostedName}";
        throw new InvalidOperationException($"{popupIdentity} not configured");
    }

    /// <summary>
    /// Represents event data for a publication event, encapsulating relevant information.
    /// </summary>
    /// <param name="content">Holds the message or information related to the publication event.</param>
    /// <param name="Context">Contains the contextual information regarding the popup associated with the event.</param>
    internal record PubEventArgs(string content, PopupContext Context);

    /// <summary>
    /// Displays a notification asynchronously for a specified duration in a UI element.
    /// </summary>
    /// <param name="Reference">Used to check if the notification host is still valid before displaying the notification.</param>
    record NofityHosted(WeakReference Reference)
    {
        SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Asynchronously displays a notification for a specified duration in a UI element.
        /// </summary>
        /// <param name="message">The content to be displayed in the notification.</param>
        /// <param name="timeSpan">The duration for which the notification will be visible.</param>
        /// <returns>A task representing the asynchronous operation of displaying the notification.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the notification host has expired and is no longer valid.</exception>
        internal async Task NotifyAsync(string message, TimeSpan timeSpan)
        {
            try
            {
                await semaphore.WaitAsync();

                if (Reference.Target is not AdornerDecorator decorator)
                {
                    throw new InvalidOperationException("notification host has expired");
                }

                UIElement uielement = default!;

                DataTemplate? datatemplate = GetNotificationTemplate(decorator);

                if (datatemplate is not null)
                {
                    uielement = new ContentControl()
                    {
                        DataContext = message,
                        ContentTemplate = datatemplate,
                        Content = message,
                    };
                }
                else
                {
                    uielement = new NotifyContainer() { DataContext = message };
                }

                AdornerLayer layer = AdornerLayer.GetAdornerLayer(decorator);

                using ContentAdorner contentAdorner = new(uielement, decorator);

                layer.Add(contentAdorner);

                TaskCompletionSource<bool> taskCompletion = new TaskCompletionSource<bool>();

                ThreadPool.QueueUserWorkItem(o =>
                {
                    Thread.Sleep(timeSpan); // wait for the specified time
                    taskCompletion.SetResult(true);
                });

                await taskCompletion.Task;

                layer.Remove(contentAdorner);
            }
            finally
            {
                _ = semaphore.Release(1);
            }
        }
    }
}
