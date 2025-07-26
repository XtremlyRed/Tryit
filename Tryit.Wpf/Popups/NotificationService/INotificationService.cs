using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Tryit.Wpf;

/// <summary>
/// Provides methods to send notifications with a message and duration. Notifications can be sent to a specific host or
/// generally.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification with a specified message and duration.
    /// </summary>
    /// <param name="message">The content of the notification to be sent.</param>
    /// <param name="timeSpan">The duration for which the notification should be displayed.</param>
    /// <returns>A task that represents the asynchronous operation of sending the notification.</returns>
    Task NotifyAsync(string message, TimeSpan timeSpan);

    /// <summary>
    /// Sends a notification asynchronously to a specified host with a message and a duration.
    /// </summary>
    /// <param name="hostedName">Specifies the recipient of the notification.</param>
    /// <param name="message">Contains the content of the notification to be sent.</param>
    /// <param name="timeSpan">Indicates how long the notification should be active or displayed.</param>
    /// <returns>Provides a task that represents the asynchronous operation.</returns>
    Task NotifyAsyncIn(string hostedName, string message, TimeSpan timeSpan);
}
