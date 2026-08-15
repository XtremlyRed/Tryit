using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using PropertyChanged;

namespace Tryit;

/// <summary>
/// Delegate representing a perceived change notification for a property.
/// The optional  <paramref name="propertyName"/>  parameter indicates which property changed.
/// </summary>
/// <param name="propertyName">Name of the property that was perceived to change. May be null.</param>
public delegate void PerceivedChangedEventHandler(string? propertyName = null);

/// <summary>
/// Interface for a collector that can raise perceived-change notifications.
/// Implementers expose the <see cref="PerceivedChanged"/> event to allow subscribers to react when
/// a property or the collection itself has changed in a way that is relevant to observers.
/// </summary>
public interface IPerceivableChanged
{
    /// <summary>
    /// Event raised when a property or the collection is perceived to have changed.
    /// Subscribers receive the name of the changed property when available.
    /// </summary>
    event PerceivedChangedEventHandler? PerceivedChanged;
}

/// <summary>
/// An ObservableCollection that listens to property changes on its items and exposes a
/// perceived-change event. Useful for UI scenarios where consumers need a single notification
/// when either the collection changes (items added/removed) or when properties of contained
/// items change.
/// </summary>
/// <typeparam name="T">Type of the items stored in the collection. Items that implement
/// <see cref="INotifyPropertyChanged"/> will be monitored for property changes.</typeparam>
public class PerceivableChangedCollector<T> : ObservableCollection<T>, IPerceivableChanged
{
    /// <summary>
    /// A list of property names that should be ignored when raising perceived-change notifications.
    /// When a property in this list changes on any item, the collector will not notify subscribers.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly List<string> ignoreNotifyMaps = [];

    /// <summary>
    /// Initializes a new instance of <see cref="PerceivableChangedCollector{T}"/> and adds the
    /// provided sequence of values to the collection.
    /// </summary>
    /// <param name="values">Initial items to add to the collection. If null, no items are added.</param>
    public PerceivableChangedCollector(IEnumerable<T> values)
        : base()
    {
        AddRange(values);
    }

    /// <summary>
    /// Default constructor. Creates an empty collector.
    /// </summary>
    public PerceivableChangedCollector()
        : base() { }

    /// <summary>
    /// Event raised when the collection or a monitored property on an item is perceived to have changed.
    /// Handlers receive the name of the affected property where available; when the collection changes the
    /// <c>Count</c> property name is used.
    /// </summary>
    public event PerceivedChangedEventHandler? PerceivedChanged;

    /// <summary>
    /// Controls whether property-changed events from items will be propagated as perceived-change notifications.
    /// Default is true.
    /// </summary>
    public bool EnablePropertyChangedNotify { get; set; } = true;

    /// <summary>
    /// Adds multiple sequences of values to the collection. Each parameter is an <see cref="IEnumerable{T}"/>
    /// instance and will be enumerated and added. Null parameter is ignored.
    /// </summary>
    /// <param name="datas">One or more sequences of items to add.</param>
    public void AddRange(params IEnumerable<T> datas)
    {
        if (datas is null)
        {
            return;
        }

        foreach (T item in datas)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Adds an array of items to the collection. This overload is provided for convenience when callers
    /// have a simple array of items to add.
    /// </summary>
    /// <param name="datas">Array of items to add. Null or empty arrays are ignored.</param>
    public void AddRange(params T[] datas)
    {
        for (int i = 0, length = datas?.Length ?? 0; i < length; i++)
        {
            Add(datas![i]);
        }
    }

    /// <summary>
    /// Internal handler that is attached to each item's <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event. It forwards the property change as a perceived-change notification unless the property is
    /// configured to be ignored or notifications are disabled.
    /// </summary>
    /// <param name="sender">Item that raised the property change.</param>
    /// <param name="e">Event arguments describing the property that changed.</param>
    private void RaiseCollectorItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (EnablePropertyChangedNotify == false || (ignoreNotifyMaps.Count > 0 && ignoreNotifyMaps.Contains(e.PropertyName!)))
        {
            return;
        }

        // Propagate the PropertyChanged event on the collection side as well.
        base.OnPropertyChanged(e);

        PerceivedChangedEventHandler? handler = PerceivedChanged;

        handler?.Invoke(e.PropertyName!);
    }

    /// <summary>
    /// Overridden to raise a perceived-change notification for the collection Count when items are added or removed.
    /// The attribute suppresses property-changed analyzer warnings for this override.
    /// </summary>
    /// <param name="e">Event arguments for the collection change.</param>
    [SuppressPropertyChangedWarnings]
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);

        PerceivedChangedEventHandler? handler = PerceivedChanged;
        handler?.Invoke(nameof(Count));
    }

    /// <summary>
    /// Clears the collection and detaches property-change handlers from all items previously in the collection.
    /// </summary>
    protected override void ClearItems()
    {
        for (int index = 0; index < Count; index++)
        {
            if (this[index] is INotifyPropertyChanged notifyProperty)
            {
                notifyProperty.PropertyChanged -= RaiseCollectorItemPropertyChanged;
            }

            if (this[index] is IPerceivableChanged perceivableChanged)
            {
                perceivableChanged.PerceivedChanged -= PerceivableChanged_PerceivedChanged;
            }
        }

        base.ClearItems();
    }

    /// <summary>
    /// Inserts an item at the specified index and attaches a property-change handler if the item implements
    /// <see cref="INotifyPropertyChanged"/>. Existing handlers are removed before attaching to avoid duplicates.
    /// </summary>
    /// <param name="index">Index at which to insert the item.</param>
    /// <param name="item">Item to insert.</param>
    protected override void InsertItem(int index, T item)
    {
        if (item is INotifyPropertyChanged notifyProperty)
        {
            notifyProperty.PropertyChanged -= RaiseCollectorItemPropertyChanged;
            notifyProperty.PropertyChanged += RaiseCollectorItemPropertyChanged;
        }
        if (item is IPerceivableChanged perceivableChanged)
        {
            perceivableChanged.PerceivedChanged -= PerceivableChanged_PerceivedChanged;
            perceivableChanged.PerceivedChanged += PerceivableChanged_PerceivedChanged;
        }
        base.InsertItem(index, item!);
    }

    /// <summary>
    /// Replaces the item at the specified index, detaching and re-attaching property-change handlers as needed.
    /// </summary>
    /// <param name="index">Index of the item to replace.</param>
    /// <param name="item">New item to set.</param>
    protected override void SetItem(int index, T item)
    {
        if (item is INotifyPropertyChanged notifyProperty)
        {
            notifyProperty.PropertyChanged -= RaiseCollectorItemPropertyChanged;
            notifyProperty.PropertyChanged += RaiseCollectorItemPropertyChanged;
        }
        if (this[index] is IPerceivableChanged perceivableChanged)
        {
            perceivableChanged.PerceivedChanged -= PerceivableChanged_PerceivedChanged;
            perceivableChanged.PerceivedChanged += PerceivableChanged_PerceivedChanged;
        }

        base.SetItem(index, item!);
    }

    /// <summary>
    /// Removes the item at the specified index and detaches its property-change handler if present.
    /// </summary>
    /// <param name="index">Index of the item to remove.</param>
    protected override void RemoveItem(int index)
    {
        if (this[index] is INotifyPropertyChanged notifyProperty)
        {
            notifyProperty.PropertyChanged -= RaiseCollectorItemPropertyChanged;
        }

        if (this[index] is IPerceivableChanged perceivableChanged)
        {
            perceivableChanged.PerceivedChanged -= PerceivableChanged_PerceivedChanged;
        }

        base.RemoveItem(index);
    }

    private void PerceivableChanged_PerceivedChanged(string? propertyName = null)
    {
        PerceivedChangedEventHandler? handler = PerceivedChanged;

        handler?.Invoke(propertyName!);
    }

    /// <summary>
    /// Registers a property name to be ignored by perceived-change notifications. Useful when certain
    /// property updates are noisy and should not trigger UI refreshes or other reactions.
    /// </summary>
    /// <param name="propertyName">Property name to ignore. Null values are ignored.</param>
    /// <returns>The current collector instance to allow fluent calls.</returns>
    public virtual PerceivableChangedCollector<T> IgnoreProperty(string propertyName)
    {
        if (propertyName is not null)
        {
            ignoreNotifyMaps.Add(propertyName);
        }

        return this;
    }

    /// <summary>
    /// Generates a disposable token that manages subscription to the collector's perceived-changed event.
    /// The returned token can be used to temporarily detach the handler (via <see cref="PerceivedChangedEventHandlerToken.EventApply"/>)
    /// and will re-attach the handler when disposed.
    /// </summary>
    /// <param name="perceivedChangedEventHandler">Handler to attach to the collector.</param>
    /// <returns>A token that controls the lifetime and temporary detachment of the handler.</returns>
    public virtual PerceivedChangedEventHandlerToken GenerateEventHandlerToken(PerceivedChangedEventHandler perceivedChangedEventHandler)
    {
        _ = perceivedChangedEventHandler ?? throw new ArgumentNullException(nameof(perceivedChangedEventHandler));
        return new PerceivedChangedEventHandlerToken(this, perceivedChangedEventHandler);
    }
}

/// <summary>
/// Token object returned by <see cref="PerceivableChangedCollector{T}.GenerateEventHandlerToken"/>.
/// The token registers the supplied handler on construction. Calling <see cref="EventApply"/> will
/// temporarily detach the handler. Disposing the token will re-attach the handler.
/// </summary>
public sealed class PerceivedChangedEventHandlerToken : IDisposable
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly IPerceivableChanged Collector;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly PerceivedChangedEventHandler perceivedChangedEventHandler;

    internal PerceivedChangedEventHandlerToken(IPerceivableChanged Collector, PerceivedChangedEventHandler perceivedChangedEventHandler)
    {
        this.Collector = Collector;
        this.perceivedChangedEventHandler = perceivedChangedEventHandler;

        // Attach the handler immediately on construction.
        Collector.PerceivedChanged += this.perceivedChangedEventHandler;
    }

    /// <summary>
    /// Temporarily removes the handler from the collector. The method removes the handler twice to ensure
    /// it is fully detached in cases where it might have been attached multiple times by mistake.
    /// Returns this token so callers can chain calls if needed.
    /// </summary>
    /// <returns>Returns the token instance for fluent usage.</returns>
    public IDisposable EventApply()
    {
        for (int i = 0; i < 2; i++)
        {
            Collector.PerceivedChanged -= this.perceivedChangedEventHandler;
        }

        return this;
    }

    /// <summary>
    /// Dispose re-attaches the handler to the collector. This pairs with <see cref="EventApply"/>
    /// to allow a scope-based temporary detachment.
    /// </summary>
    void IDisposable.Dispose()
    {
        Collector.PerceivedChanged += this.perceivedChangedEventHandler;
    }
}
