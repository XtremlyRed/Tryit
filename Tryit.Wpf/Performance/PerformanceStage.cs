using System.Collections;
using System.Windows;
using System.Windows.Media.Animation;

#pragma warning disable CS9113

namespace Tryit.Wpf;

/// <summary>
/// Represents a collection of Performer objects that defines a stage for managing and coordinating performance
/// animations in response to specific UI events within a WPF application.
/// </summary>
/// <remarks>PerformanceStage is designed to facilitate the orchestration of multiple animation performers that
/// are triggered by configurable stage events, such as Loaded, MouseEnter, or DataContextChanged. It integrates with
/// the WPF dependency property system and supports collection semantics, allowing dynamic addition and management of
/// performers. This class is typically used in scenarios where complex UI transitions or interactive behaviors are
/// required, and provides mechanisms to attach to UI elements and respond to lifecycle or input events.
/// PerformanceStage inherits from FreezableCollection, enabling features such as data binding, cloning, and thread
/// safety for collection modifications.</remarks>
public class PerformanceStage : FreezableCollection<Performer>, ICollection<Performer>, IList
{
    /// <summary>
    /// Adds a performer to the collection if it is not already present based on specific criteria.
    /// </summary>
    /// <remarks>A performer is considered a duplicate and will not be added if an existing
    /// TransitionPerformer has the same TransitionOn value or an existing ColorPerformer has the same ColorOn value as
    /// the performer being added. If the item is not a Performer, the method does nothing and returns 0.</remarks>
    /// <param name="item">The performer to add to the collection. Must be a non-null instance of the Performer type or a derived type.</param>
    /// <returns>1 if the performer was successfully added; otherwise, 0.</returns>
    int IList.Add(object? item)
    {
        if (item is not Performer performer)
        {
            return 0;
        }

        if (IsExist<TransitionPerformer>(this, performer, (c, p2) => c.TransitionOn == p2.TransitionOn))
        {
            return 0;
        }

        if (IsExist<ColorPerformer>(this, performer, (c, p2) => c.ColorOn == p2.ColorOn))
        {
            return 0;
        }

        base.Add(performer);

        return 1;

        static bool IsExist<T>(ICollection<Performer> performers, Performer performer, Func<T, T, bool> filter)
            where T : Performer
        {
            return performer is T input && performers.Any(x => x is T tar && filter(tar, input));
        }
    }

    /// <summary>
    /// Gets or sets the stage transition event associated with this element.
    /// </summary>
    public TransitionEvent StageEvent
    {
        get => (TransitionEvent)GetValue(StageEventProperty);
        set => SetValue(StageEventProperty, value);
    }

    /// <summary>
    /// Identifies the StageEvent dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the StageEvent property with the Windows
    /// Presentation Foundation (WPF) property system. It is typically used when calling methods such as SetValue or
    /// GetValue on instances of PerformanceStage.</remarks>
    public static readonly DependencyProperty StageEventProperty = DependencyProperty.Register(nameof(StageEvent), typeof(TransitionEvent), typeof(PerformanceStage), new PropertyMetadata(TransitionEvent.Loaded));

    #region private

    /// <summary>
    /// Retrieves the StoryboardMapper instance associated with the specified dependency object.
    /// </summary>
    /// <param name="obj">The DependencyObject from which to retrieve the associated StoryboardMapper. Cannot be null.</param>
    /// <returns>The StoryboardMapper instance associated with the specified object, or null if no association exists.</returns>
    private static StoryboardMapper GetStoryboardMapper(DependencyObject obj)
    {
        return (StoryboardMapper)obj.GetValue(StoryboardMapperProperty);
    }

    /// <summary>
    /// Sets the storyboard mapper associated with the specified dependency object.
    /// </summary>
    /// <param name="obj">The dependency object on which to set the storyboard mapper. Cannot be null.</param>
    /// <param name="value">The storyboard mapper to associate with the dependency object. Can be null to clear the existing mapper.</param>
    private static void SetStoryboardMapper(DependencyObject obj, StoryboardMapper value)
    {
        obj.SetValue(StoryboardMapperProperty, value);
    }

    /// <summary>
    /// Identifies the StoryboardMapper attached dependency property.
    /// </summary>
    private static readonly DependencyProperty StoryboardMapperProperty = DependencyProperty.RegisterAttached("StoryboardMapper", typeof(StoryboardMapper), typeof(PerformanceStage), new PropertyMetadata(null));

    /// <summary>
    /// Represents a mapping between transition events and their associated storyboards.
    /// </summary>
    /// <remarks>This class is typically used to associate specific storyboard animations with particular
    /// transition events in a user interface workflow. It inherits all behavior from the generic Dictionary class and
    /// does not add additional functionality.</remarks>
    private class StoryboardMapper : Dictionary<TransitionEvent, Storyboard> { }

    #endregion

    /// <summary>
    /// Gets the object to which this behavior is attached.
    /// </summary>
    /// <remarks>The associated object is typically set when the behavior is attached and cleared when it is
    /// detached. Access this property to interact with the object that the behavior is currently extending.</remarks>
    protected DependencyObject AssociatedObject { get; private set; } = default!;

    /// <summary>
    /// Initializes the transition by associating it with the specified dependency object and configuring event handlers
    /// and storyboards based on the current transition event.
    /// </summary>
    /// <remarks>This method sets up event handlers for the specified transition event (such as Loaded,
    /// DataContextChanged, MouseEnter, MouseLeave, GotFocus, or LostFocus) on the associated UI element. It also
    /// ensures that a storyboard is created and populated with animations for the transition. This method should be
    /// called before the transition is used to ensure proper event and animation setup.</remarks>
    /// <param name="dependencyObject">The dependency object to associate with the transition. Must not be null and should be a UIElement to enable
    /// event handling.</param>
    internal void OnInitialize(DependencyObject dependencyObject)
    {
        if ((AssociatedObject = dependencyObject) is not UIElement element)
        {
            return;
        }

        if (StageEvent == TransitionEvent.Loaded && element is FrameworkElement framework1)
        {
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(framework1, nameof(FrameworkElement.Loaded), OnLoaded);
        }
        else if (StageEvent == TransitionEvent.DataContextChanged && element is FrameworkElement framework2)
        {
            DataContextChangedEventManager.AddListener(framework2, OnDataContextChanged);
        }
        else if (StageEvent == TransitionEvent.MouseEnter)
        {
            WeakEventManager<UIElement, MouseEventArgs>.AddHandler(element, nameof(UIElement.MouseEnter), OnMouseEnter);
        }
        else if (StageEvent == TransitionEvent.MouseLeave)
        {
            WeakEventManager<UIElement, MouseEventArgs>.AddHandler(element, nameof(UIElement.MouseLeave), OnMouseLeave);
        }
        else if (StageEvent == TransitionEvent.GotFocus)
        {
            WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(element, nameof(UIElement.GotFocus), OnGotFocus);
        }
        else if (StageEvent == TransitionEvent.LostFocus)
        {
            WeakEventManager<UIElement, RoutedEventArgs>.AddHandler(element, nameof(UIElement.LostFocus), OnLostFocus);
        }

        if (GetStoryboardMapper(AssociatedObject!) is not StoryboardMapper storyboardMaps)
        {
            SetStoryboardMapper(AssociatedObject!, storyboardMaps = new());
        }

        if (storyboardMaps.TryGetValue(StageEvent, out Storyboard? storyboard) == false)
        {
            storyboardMaps[StageEvent] = storyboard = new Storyboard();
        }

        storyboard.Children.Clear();

        for (int i = Count - 1; i >= 0; i--)
        {
            if (this[i].AnimationBuild(AssociatedObject) is AnimationTimeline animationTimeline)
            {
                storyboard.Children.Add(animationTimeline);
            }
        }
    }

    /// <summary>
    /// Handles the Loaded routed event for a UI element and begins the associated storyboard transition if configured.
    /// </summary>
    /// <remarks>This method is intended to be used as an event handler for the Loaded event of a WPF element.
    /// It checks for a storyboard transition mapped to the Loaded event and begins the animation if one is
    /// found.</remarks>
    /// <param name="sender">The source of the event, typically the UI element that has been loaded.</param>
    /// <param name="e">The event data for the Loaded routed event.</param>
    internal void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (StageEvent == TransitionEvent.Loaded && sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.Loaded, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    /// <summary>
    /// Handles the DataContextChanged event by starting the associated storyboard transition, if one is defined for the
    /// sender.
    /// </summary>
    /// <remarks>If a storyboard transition is mapped to the DataContextChanged event for the sender, this
    /// method begins that storyboard. This enables visual transitions when the data context of a UI element
    /// changes.</remarks>
    /// <param name="sender">The object on which the DataContextChanged event occurred. Expected to be a DependencyObject.</param>
    /// <param name="args">The event data associated with the DataContextChanged event.</param>
    private void OnDataContextChanged(object? sender, DataContextChangedEventManager.DependencyPropertyChangedEventArgs args)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.DataContextChanged, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    /// <summary>
    /// Handles the mouse enter event by starting the associated storyboard animation, if one is mapped to the sender.
    /// </summary>
    /// <remarks>This method is typically used to trigger visual transitions or animations when the mouse
    /// pointer enters a UI element. If no storyboard is mapped to the sender for the mouse enter event, no action is
    /// taken.</remarks>
    /// <param name="sender">The source of the event. Expected to be a DependencyObject for which a storyboard may be mapped.</param>
    /// <param name="e">The event data associated with the mouse event.</param>
    private void OnMouseEnter(object? sender, MouseEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.MouseEnter, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    /// <summary>
    /// Handles the MouseLeave event by triggering any associated storyboard transition for the specified element.
    /// </summary>
    /// <remarks>If the sender has a storyboard mapped to the MouseLeave transition, this method begins the
    /// associated storyboard. No action is taken if no storyboard is mapped or if the sender is not a
    /// DependencyObject.</remarks>
    /// <param name="sender">The source of the event, typically a UI element that the mouse pointer has left. Must be a DependencyObject to
    /// trigger a storyboard transition.</param>
    /// <param name="e">The event data associated with the mouse event.</param>
    private void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.MouseLeave, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    /// <summary>
    /// Handles the GotFocus event by starting the associated storyboard transition for the focused element, if one is
    /// defined.
    /// </summary>
    /// <remarks>If the focused element has a storyboard mapped to the GotFocus event, this method begins that
    /// storyboard to provide a visual transition. No action is taken if no storyboard is associated with the
    /// event.</remarks>
    /// <param name="sender">The source of the event, typically the element that received focus. Must be a DependencyObject to trigger a
    /// storyboard transition.</param>
    /// <param name="e">The event data for the GotFocus routed event.</param>
    private void OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.GotFocus, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    /// <summary>
    /// Handles the LostFocus event by triggering the associated storyboard transition, if one is defined for the
    /// sender.
    /// </summary>
    /// <remarks>If no storyboard is mapped for the LostFocus event on the sender, this method performs no
    /// action.</remarks>
    /// <param name="sender">The source of the event. Expected to be a DependencyObject for which a storyboard transition may be mapped.</param>
    /// <param name="e">The event data associated with the LostFocus event.</param>
    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.LostFocus, out Storyboard? storyboard))
            {
                storyboard.Begin();
            }
        }
    }
}

/// <summary>
/// Manages weak event listeners for DataContextChanged events on FrameworkElement instances, enabling event
/// handlers to be attached without creating strong references that could prevent garbage collection.
/// </summary>
/// <remarks>This class is used internally to support the weak event pattern for DataContextChanged
/// notifications in WPF. It allows event handlers to be registered and removed for data context changes on
/// FrameworkElement objects without causing memory leaks. Typically, you do not use this class directly; instead,
/// use the provided static AddHandler and RemoveHandler methods to manage event subscriptions.</remarks>
public class DataContextChangedEventManager : WeakEventManager
{
    /// <summary>
    /// Begins listening for changes to the data context on the specified source object.
    /// </summary>
    /// <remarks>This method attaches a handler to the DataContextChanged event if the source is a
    /// FrameworkElement. No action is taken if the source does not support data context notifications.</remarks>
    /// <param name="source">The object to monitor for data context changes. Must be a FrameworkElement to support data context
    /// notifications.</param>
    protected override void StartListening(object source)
    {
        if (source is FrameworkElement frameworkElement)
        {
            frameworkElement.DataContextChanged += OnDataContextChanged;
        }
    }

    /// <summary>
    /// Handles the event that occurs when the data context of the associated object changes.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object whose data context has changed.</param>
    /// <param name="e">An object that contains the event data, including information about the old and new values of the data
    /// context.</param>
    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        base.DeliverEvent(sender, new DependencyPropertyChangedEventArgs(e.Property, e.OldValue, e.NewValue));
    }

    /// <summary>
    /// Stops listening for DataContextChanged events on the specified source object.
    /// </summary>
    /// <remarks>If the specified source is not a FrameworkElement, this method has no
    /// effect.</remarks>
    /// <param name="source">The object from which to detach the DataContextChanged event handler. Must be a FrameworkElement.</param>
    protected override void StopListening(object source)
    {
        if (source is FrameworkElement frameworkElement)
        {
            frameworkElement.DataContextChanged -= OnDataContextChanged;
        }
    }

    /// <summary>
    /// Registers a listener to receive notifications when any dependency property on the specified FrameworkElement
    /// changes.
    /// </summary>
    /// <remarks>This method attaches a weak event listener, which does not prevent the source element or the
    /// handler from being garbage collected. Use this method to observe property changes without creating strong
    /// references that could lead to memory leaks.</remarks>
    /// <param name="source">The FrameworkElement to monitor for dependency property changes. Cannot be null.</param>
    /// <param name="dependencyPropertyChangedEventHandler">The event handler to invoke when a dependency property value changes on the source element. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if source or dependencyPropertyChangedEventHandler is null.</exception>
    public static void AddListener(FrameworkElement source, DependencyPropertyChangedEventHandler dependencyPropertyChangedEventHandler)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));

        _ = dependencyPropertyChangedEventHandler ?? throw new ArgumentNullException(nameof(dependencyPropertyChangedEventHandler));

        var listener = new DelegateWeakListener(dependencyPropertyChangedEventHandler);

        source.SetValue(DelegateWeakListenerProperty, listener);

        CurrentManager.ProtectedAddListener(source, listener);
    }

    /// <summary>
    /// Removes the delegate weak listener associated with the specified source element, if one exists.
    /// </summary>
    /// <param name="source">The framework element from which to remove the delegate weak listener. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if the source parameter is null.</exception>
    public static void RemoveListener(FrameworkElement source)
    {
        _ = source ?? throw new ArgumentNullException(nameof(source));

        if (source.GetValue(DelegateWeakListenerProperty) is DelegateWeakListener delegateWeakListener)
        {
            CurrentManager.ProtectedRemoveListener(source, delegateWeakListener);
        }
    }

    /// <summary>
    /// Gets the current instance of the DataContextChangedEventManager for managing DataContextChanged event
    /// listeners.
    /// </summary>
    /// <remarks>This property is intended for use by the event manager infrastructure to coordinate
    /// event delivery. It returns a shared manager instance for the current application domain.</remarks>
    protected static DataContextChangedEventManager CurrentManager
    {
        get
        {
            if (WeakEventManager.GetCurrentManager(typeof(DataContextChangedEventManager)) is not DataContextChangedEventManager cachedManager)
            {
                cachedManager = new DataContextChangedEventManager();
                WeakEventManager.SetCurrentManager(typeof(DataContextChangedEventManager), cachedManager);
            }
            return cachedManager;
        }
    }

    /// <summary>
    /// Represents a weak event listener that delegates property change notifications to a specified handler.
    /// </summary>
    /// <remarks>This class is typically used to attach event handlers to dependency property change events
    /// without creating strong references that could prevent garbage collection. It is useful in scenarios where memory
    /// leaks due to event subscriptions need to be avoided.</remarks>
    /// <param name="handler">The delegate to invoke when a dependency property change event is received. Cannot be null.</param>
    class DelegateWeakListener(DependencyPropertyChangedEventHandler handler) : IWeakEventListener
    {
        /// <summary>
        /// Handles a weak event by invoking the associated handler if the event arguments are of type
        /// DependencyPropertyChangedEventArgs.
        /// </summary>
        /// <remarks>This method is typically called by a WeakEventManager to deliver events to the
        /// listener. Only events with event arguments of type DependencyPropertyChangedEventArgs are processed; all
        /// others are ignored.</remarks>
        /// <param name="managerType">The type of the WeakEventManager that is handling the event. Used to identify the event manager responsible
        /// for this event.</param>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">The event data associated with the event. Must be of type DependencyPropertyChangedEventArgs to be handled.</param>
        /// <returns>true if the event was handled; otherwise, false.</returns>
        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (e is DependencyPropertyChangedEventArgs args)
            {
                handler(sender, args);
                return true;
            }
            return false;
        }
    }

    static readonly DependencyProperty DelegateWeakListenerProperty = DependencyProperty.RegisterAttached("DelegateWeakListener", typeof(DelegateWeakListener), typeof(DataContextChangedEventManager), new PropertyMetadata(null));

    /// <summary>
    /// Provides data for events that report changes to a dependency property value.
    /// </summary>
    /// <remarks>This class is typically used in event handlers for property changed events to provide
    /// information about the specific property that changed and its old and new values.</remarks>
    /// <param name="Property">The dependency property that changed.</param>
    /// <param name="OldValue">The value of the property before the change occurred.</param>
    /// <param name="NewValue">The value of the property after the change occurred.</param>
    public class DependencyPropertyChangedEventArgs(DependencyProperty Property, object OldValue, object NewValue) : EventArgs { }

    /// <summary>
    /// Represents the method that handles events raised when the value of a dependency property changes.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object where the property value has changed.</param>
    /// <param name="changedEventArgs">An object that contains data about the property change event, including the old and new values.</param>
    public delegate void DependencyPropertyChangedEventHandler(object sender, DependencyPropertyChangedEventArgs changedEventArgs);
}
