using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Provides methods to retrieve events of specified types. Supports both synchronous and asynchronous event retrieval.
/// </summary>
public interface IEventManager
{
    /// <summary>
    /// Retrieves an event of a specified type.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the event to be retrieved.</typeparam>
    /// <returns>Returns an instance of the event corresponding to the specified type.</returns>
    IEvent<T> GetEvent<T>();

    /// <summary>
    /// Retrieves an asynchronous event of a specified type.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the event being retrieved.</typeparam>
    /// <returns>An asynchronous event that can be awaited for notifications.</returns>
    IAsyncEvent<T> GetAsyncEvent<T>();
}

/// <summary>
/// Defines an interface for event subscription and publishing, allowing for event-driven communication.
/// </summary>
/// <typeparam name="TEvent">Represents the type of event data that can be handled by the event system.</typeparam>
public interface IEvent<TEvent>
{
    /// <summary>
    /// Subscribes to an event with a specified action and thread policy for event handling.
    /// </summary>
    /// <param name="subscribe">Defines the action to be executed when the event is triggered.</param>
    /// <param name="threadPolicy">Specifies the threading model to be used for executing the event handler.</param>
    /// <returns>Returns an unsubscribe mechanism to stop receiving events.</returns>
    IUnsubscrible Subscribe(Action<TEvent> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current);

    /// <summary>
    /// Sends an event to subscribers for processing. This allows for the event-driven architecture to function
    /// effectively.
    /// </summary>
    /// <param name="event">The event to be published to all interested subscribers for handling.</param>
    void Publish(TEvent @event);

    /// <summary>
    /// Subscribes to a specified channel to receive events. It allows specifying the thread policy for event handling.
    /// </summary>
    /// <param name="channel">Specifies the channel to which events will be subscribed.</param>
    /// <param name="subscribe">Defines the action to be executed when an event is received.</param>
    /// <param name="threadPolicy">Determines the threading model for event handling.</param>
    /// <returns>Returns an unsubscribe object to stop receiving events.</returns>
    IUnsubscrible Subscribe(string channel, Action<TEvent> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current);

    /// <summary>
    /// Sends an event to a specified communication channel.
    /// </summary>
    /// <param name="channel">Identifies the destination for the event being sent.</param>
    /// <param name="event">Represents the data or message that is being transmitted.</param>
    void Publish(string channel, TEvent @event);
}

/// <summary>
/// Defines an interface for handling asynchronous events with subscription and publishing capabilities.
/// </summary>
/// <typeparam name="TEvent">Represents the type of data associated with the event being handled.</typeparam>
public interface IAsyncEvent<TEvent>
{
    /// <summary>
    /// Subscribes to an event with an asynchronous callback and a specified threading policy.
    /// </summary>
    /// <param name="subscribe">The asynchronous callback function to be invoked when the event occurs.</param>
    /// <param name="threadPolicy">Determines the threading context in which the callback will be executed.</param>
    /// <returns>An object that allows for unsubscribing from the event.</returns>
    IUnsubscrible Subscribe(Func<TEvent, Task> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current);

    /// <summary>
    /// Asynchronously publishes an event to a messaging system or service.
    /// </summary>
    /// <param name="event">The event to be published, which contains the data to be sent.</param>
    /// <returns>A task representing the asynchronous operation of publishing the event.</returns>

    Task PublishAsync(TEvent @event);

    /// <summary>
    /// Subscribes to a specified channel to receive events asynchronously. It allows specifying the threading policy
    /// for event handling.
    /// </summary>
    /// <param name="channel">Specifies the channel to which events will be subscribed.</param>
    /// <param name="subscribe">Defines the asynchronous method that will handle the events received from the specified channel.</param>
    /// <param name="threadPolicy">Indicates the threading policy to be used for executing the event handling method.</param>
    /// <returns>Returns an object that allows for unsubscribing from the channel.</returns>
    IUnsubscrible Subscribe(string channel, Func<TEvent, Task> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current);

    /// <summary>
    /// Asynchronously publishes an event to a specified channel.
    /// </summary>
    /// <param name="channel">Specifies the destination for the event to be published.</param>
    /// <param name="event">Represents the event data that will be sent to the specified channel.</param>
    /// <returns>Returns a task that represents the asynchronous operation.</returns>
    Task PublishAsync(string channel, TEvent @event);
}

/// <summary>
/// Defines policies for managing event threads, including tracking the current state, publishing threads, and creating
/// new threads. These policies facilitate concurrent execution and content distribution.
/// </summary>
public enum EventThreadPolicy
{
    /// <summary>
    /// Represents the current state or value in a given context. It is often used to track the present condition or
    /// status.
    /// </summary>
    Current,

    /// <summary>
    /// Handles the publishing of threads in a system. It manages the creation and distribution of thread-related
    /// content.
    /// </summary>
    PublishThread,

    /// <summary>
    /// Creates a new thread for concurrent execution. This allows for parallel processing of tasks.
    /// </summary>
    NewThread,
}

/// <summary>
/// Removes the current instance from subscriptions, stopping notifications or updates. This is part of the IDisposable
/// interface.
/// </summary>
public interface IUnsubscrible : IDisposable
{
    /// <summary>
    /// Removes the current instance from any subscriptions. This effectively stops receiving notifications or updates.
    /// </summary>
    void Unsubscribe();
}
