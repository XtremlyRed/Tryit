using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Manages asynchronous and synchronous events, allowing retrieval or creation of events of specified types. Ensures
/// thread safety during event creation.
/// </summary>
public class EventManager : IEventManager
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<object> asyncEvents = new List<object>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<object> syncEvents = new List<object>();

    /// <summary>
    /// Retrieves or creates an asynchronous event of a specified type.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the asynchronous event to retrieve or create.</typeparam>
    /// <returns>An instance of an asynchronous event for the specified type.</returns>
    public IAsyncEvent<T> GetAsyncEvent<T>()
    {
        if (TryFind<T>(asyncEvents, out var asyncEvent) == false)
        {
            lock (this)
            {
                if (TryFind<T>(asyncEvents, out asyncEvent) == false)
                {
                    asyncEvent = new AsyncEvent<T>();
                    asyncEvents.Add(asyncEvent);
                }
            }
        }

        return asyncEvent;

        static bool TryFind<T1>(List<object> asyncEvents, out IAsyncEvent<T1> asyncEvent)
        {
            for (int i = asyncEvents.Count - 1; i >= 0; i--)
            {
                if (asyncEvents[i] is IAsyncEvent<T1> asyncEvent1)
                {
                    asyncEvent = asyncEvent1;

                    return true;
                }
            }
            asyncEvent = default!;
            return false;
        }
    }

    /// <summary>
    /// Retrieves or creates an event of a specified type, ensuring thread safety during the creation process.
    /// </summary>
    /// <typeparam name="T">Specifies the type of the event to retrieve or create.</typeparam>
    /// <returns>Returns an instance of the event for the specified type.</returns>
    public IEvent<T> GetEvent<T>()
    {
        if (TryFind<T>(syncEvents, out var syncEvent) == false)
        {
            lock (this)
            {
                if (TryFind<T>(syncEvents, out syncEvent) == false)
                {
                    syncEvent = new Event<T>();
                    syncEvents.Add(syncEvent);
                }
            }
        }

        return syncEvent;

        static bool TryFind<T1>(List<object> syncEvents, out IEvent<T1> asyncEvent)
        {
            for (int i = syncEvents.Count - 1; i >= 0; i--)
            {
                if (syncEvents[i] is IEvent<T1> asyncEvent1)
                {
                    asyncEvent = asyncEvent1;

                    return true;
                }
            }
            asyncEvent = default!;
            return false;
        }
    }
}

/// <summary>
/// Handles event publishing and subscription management for a specified channel, allowing actions to be triggered on
/// event occurrences.
/// </summary>
/// <typeparam name="T">Represents the type of event data that can be published and subscribed to within the event system.</typeparam>
public class Event<T> : IEvent<T>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IDictionary<string, List<object>> eventMaps = new ConcurrentDictionary<string, List<object>>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const string COMMON_CHANNEL = "{5466872F-B015-4CE0-A640-6FBE2A986B1F}";

    /// <summary>
    /// Sends an event to a predefined common channel for processing.
    /// </summary>
    /// <param name="event">The event to be published for handling in the system.</param>
    public void Publish(T @event)
    {
        Publish(COMMON_CHANNEL, @event);
    }

    /// <summary>
    /// Publishes an event to a specified channel, invoking all subscriptions associated with that channel.
    /// </summary>
    /// <param name="channel">Specifies the channel to which the event is published.</param>
    /// <param name="event">Represents the event data that will be sent to subscribers.</param>
    /// <exception cref="ArgumentNullException">Thrown when the channel parameter is null.</exception>
    public void Publish(string channel, T @event)
    {
        _ = channel ?? throw new ArgumentNullException(nameof(channel));

        if (eventMaps.TryGetValue(channel, out List<object>? subs) == false)
        {
            eventMaps[channel] = subs = new List<object>();
        }

        for (int i = subs.Count - 1; i >= 0; i--)
        {
            if (i >= subs.Count)
            {
                continue;
            }

            if (subs[i] is Subscription<T> sub)
            {
                sub.Invoke(@event);
            }
        }
    }

    /// <summary>
    /// Subscribes to an event with a specified action and thread policy. It returns an unsubscribe interface for
    /// managing the subscription.
    /// </summary>
    /// <param name="subscribe">The action to be executed when the event is triggered.</param>
    /// <param name="threadPolicy">Determines the thread context in which the action will be executed.</param>
    /// <returns>An interface that allows for unsubscribing from the event.</returns>
    public IUnsubscrible Subscribe(Action<T> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current)
    {
        return Subscribe(COMMON_CHANNEL, subscribe, threadPolicy);
    }

    /// <summary>
    /// Subscribes to a specified channel with a callback action and a threading policy.
    /// </summary>
    /// <param name="channel">Specifies the channel to which the subscription is made.</param>
    /// <param name="subscribe">Defines the action to be executed when an event is published on the specified channel.</param>
    /// <param name="threadPolicy">Indicates the threading policy for executing the callback action.</param>
    /// <returns>Returns an object that allows for unsubscribing from the channel.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the channel parameter is null.</exception>
    public IUnsubscrible Subscribe(string channel, Action<T> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current)
    {
        _ = channel ?? throw new ArgumentNullException(nameof(channel));

        if (eventMaps.TryGetValue(channel, out List<object>? subs) == false)
        {
            eventMaps[channel] = subs = new List<object>();
        }

        Subscription<T> sub = new(channel, subscribe, threadPolicy, SynchronizationContext.Current);

        subs.Add(sub);

        return new Unsubscrible(subs, sub);
    }

    /// <summary>
    /// Handles subscriptions to a specified channel with a defined threading policy for execution.
    /// </summary>
    /// <typeparam name="TE">Represents the type of the input value used during the subscription process.</typeparam>
    /// <param name="Channel">Specifies the channel to which the subscription is made.</param>
    /// <param name="Subscribe">Defines the action to be executed when a subscription event occurs.</param>
    /// <param name="ThreadPolicy">Indicates the threading policy that determines how the subscription action is executed.</param>
    /// <param name="Context">Provides the synchronization context for executing the subscription action if needed.</param>
    private record Subscription<TE>(string Channel, Action<TE> Subscribe, EventThreadPolicy ThreadPolicy, SynchronizationContext? Context)
    {
        /// <summary>
        /// Executes a subscription method based on the specified threading policy.
        /// </summary>
        /// <param name="parameter">The input value used for the subscription process.</param>
        public void Invoke(TE parameter)
        {
            switch (ThreadPolicy)
            {
                case EventThreadPolicy.Current:
                    Context?.Post((_) => Subscribe(parameter), null);
                    break;
                case EventThreadPolicy.PublishThread:
                    Subscribe(parameter);
                    break;
                case EventThreadPolicy.NewThread:
                    _ = ThreadPool.QueueUserWorkItem(_ => Subscribe(parameter));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles asynchronous event publishing and subscription management across specified channels.
/// </summary>
/// <typeparam name="T">Represents the type of event data that will be published and subscribed to.</typeparam>
public class AsyncEvent<T> : IAsyncEvent<T>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IDictionary<string, List<object>> eventMaps = new ConcurrentDictionary<string, List<object>>();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private const string COMMON_CHANNEL = "{34ED5A8B-F218-45D9-8E4A-EE60EC5563AD}";

    /// <summary>
    /// Asynchronously publishes an event to a common channel.
    /// </summary>
    /// <param name="event">The event to be published to the specified channel.</param>
    /// <returns>This method returns a Task representing the asynchronous operation.</returns>
    public async Task PublishAsync(T @event)
    {
        await PublishAsync(COMMON_CHANNEL, @event);
    }

    /// <summary>
    /// Asynchronously publishes an event to a specified channel, invoking all relevant subscriptions.
    /// </summary>
    /// <param name="channel">Specifies the channel to which the event will be published.</param>
    /// <param name="event">Represents the event data that will be sent to the subscribers.</param>
    /// <returns>This method returns a Task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the channel parameter is null.</exception>
    public async Task PublishAsync(string channel, T @event)
    {
        _ = channel ?? throw new ArgumentNullException(nameof(channel));

        if (eventMaps.TryGetValue(channel, out List<object>? subs) == false)
        {
            eventMaps[channel] = subs = new List<object>();
        }

        for (int i = subs.Count - 1; i >= 0; i--)
        {
            if (i >= subs.Count)
            {
                continue;
            }

            if (subs[i] is SubscriptionAsync<T> sub)
            {
                await sub.InvokeAsync(@event);
            }
        }
    }

    /// <summary>
    /// Subscribes to an event with an asynchronous callback and a specified threading policy.
    /// </summary>
    /// <param name="subscribe">The asynchronous callback function to be invoked when the event occurs.</param>
    /// <param name="threadPolicy">Determines the threading context in which the callback will be executed.</param>
    /// <returns>An object that allows for unsubscribing from the event.</returns>
    public IUnsubscrible Subscribe(Func<T, Task> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current)
    {
        return Subscribe(COMMON_CHANNEL, subscribe, threadPolicy);
    }

    /// <summary>
    /// Allows subscribing to a specified channel with an asynchronous callback and a defined threading policy.
    /// </summary>
    /// <param name="channel">Specifies the channel to which the subscription is made.</param>
    /// <param name="subscribe">Defines the asynchronous callback to be invoked when an event occurs on the specified channel.</param>
    /// <param name="threadPolicy">Indicates the threading policy to be used for executing the callback.</param>
    /// <returns>Returns an object that allows for unsubscribing from the channel.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the channel parameter is null.</exception>
    public IUnsubscrible Subscribe(string channel, Func<T, Task> subscribe, EventThreadPolicy threadPolicy = EventThreadPolicy.Current)
    {
        _ = channel ?? throw new ArgumentNullException(nameof(channel));

        if (eventMaps.TryGetValue(channel, out List<object>? subs) == false)
        {
            eventMaps[channel] = subs = new List<object>();
        }

        SubscriptionAsync<T> sub = new(channel, subscribe, threadPolicy, SynchronizationContext.Current);

        subs.Add(sub);

        return new Unsubscrible(subs, sub);
    }

    /// <summary>
    /// Handles asynchronous subscriptions to a specified channel with a defined threading policy.
    /// </summary>
    /// <typeparam name="TE">Represents the type of the input value used in the subscription process.</typeparam>
    /// <param name="Channel">Specifies the channel to which the subscription is made.</param>
    /// <param name="Subscribe">Defines the asynchronous action to be executed when an event occurs.</param>
    /// <param name="ThreadPolicy">Determines the threading behavior for executing the subscription action.</param>
    /// <param name="Context">Provides the synchronization context for managing thread execution.</param>
    private record SubscriptionAsync<TE>(string Channel, Func<TE, Task> Subscribe, EventThreadPolicy ThreadPolicy, SynchronizationContext? Context)
    {
        /// <summary>
        /// Executes an asynchronous operation based on the specified threading policy.
        /// </summary>
        /// <param name="parameter">The input value used in the subscription process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(TE parameter)
        {
            switch (ThreadPolicy)
            {
                case EventThreadPolicy.Current:
                    Context?.Post(async (_) => await Subscribe(parameter), null);
                    break;
                case EventThreadPolicy.PublishThread:
                    await Subscribe(parameter);
                    break;
                case EventThreadPolicy.NewThread:
                    _ = ThreadPool.QueueUserWorkItem(async _ => await Subscribe(parameter));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles unsubscribing from events using a list of event mappings and a specific event. It releases resources by
/// removing the event from the list.
/// </summary>
public class Unsubscrible : IUnsubscrible
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private List<object> eventMaps;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object @event;

    /// <summary>
    /// Initializes an instance for unsubscribing from events using a list of event mappings and a specific event.
    /// </summary>
    /// <param name="eventMaps">A collection of mappings that define the events to unsubscribe from.</param>
    /// <param name="event">The specific event that is targeted for unsubscription.</param>
    public Unsubscrible(List<object> eventMaps, object @event)
    {
        this.eventMaps = eventMaps;
        this.@event = @event;
    }

    /// <summary>
    /// Releases resources used by the object. It removes the event from eventMaps and sets eventMaps and event to null.
    /// </summary>
    void IDisposable.Dispose()
    {
        if (eventMaps is not null && eventMaps.Count > 0 && @event is not null)
        {
            _ = eventMaps.Remove(@event);
            eventMaps = null!;
            @event = null!;
        }
    }

    /// <summary>
    /// Unsubscribes from an event or service by calling the Dispose method on the IDisposable interface. This
    /// effectively releases resources.
    /// </summary>
    void IUnsubscrible.Unsubscribe()
    {
        ((IDisposable)this).Dispose();
    }
}
