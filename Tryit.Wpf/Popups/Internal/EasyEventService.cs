using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tryit.Wpf;

/// <summary>
/// A class that facilitates event publication and subscription, allowing subscribers to receive events in reverse
/// order.
/// </summary>
/// <typeparam name="T">This type parameter defines the type of data that subscribers will receive when an event is published.</typeparam>
internal class EasyEvent<T>
{
    /// <summary>
    /// A private, read-only list that stores event mappings. It is not visible in the debugger.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly List<object> eventMaps = new();

    /// <summary>
    /// Handles the publication of an event to all registered subscribers in reverse order.
    /// </summary>
    /// <param name="event">Represents the data or information that is being sent to the subscribers.</param>
    public void Publish(T @event)
    {
        for (int i = eventMaps.Count - 1; i >= 0; i--)
        {
            if (eventMaps[i] is Subscription<T> sub)
            {
                sub.Invoke(@event);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="subscribe"></param>
    /// <returns></returns>
    public IDisposable Subscribe(Action<T> subscribe)
    {
        Subscription<T> sub = new(subscribe, SynchronizationContext.Current);

        eventMaps.Add(sub);

        return new Unsubscrible(eventMaps, sub);
    }

    /// <summary>
    /// Represents a subscription that can invoke an action with a specified parameter.
    /// </summary>
    /// <typeparam name="TE">This type parameter is used to define the type of the parameter that the action will accept.</typeparam>
    /// <param name="Subscribe">This parameter is an action that will be executed when the subscription is invoked with a specific parameter.</param>
    /// <param name="Context">This parameter provides a synchronization context for executing the action, allowing for thread-safe operations.</param>
    private record Subscription<TE>(Action<TE> Subscribe, SynchronizationContext? Context)
    {
        public void Invoke(TE parameter)
        {
            Subscribe(parameter);
        }
    }

    /// <summary>
    /// Represents a subscription that can be disposed of, removing an event from a list of event mappings.
    /// </summary>
    /// <param name="eventMaps">A collection of event mappings that may contain the event to be removed.</param>
    /// <param name="event">The specific event that will be removed from the collection upon disposal.</param>
    public record Unsubscrible(List<object> eventMaps, object @event) : IDisposable
    {
        /// <summary>
        /// Releases resources used by the object. It removes the event from the eventMaps collection if it exists.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (eventMaps is not null && eventMaps.Count > 0 && @event is not null)
            {
                _ = eventMaps.Remove(@event);
            }
        }
    }
}
