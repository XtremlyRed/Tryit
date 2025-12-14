using System.Collections;
using System.Windows;
using System.Windows.Media.Animation;

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
    public static readonly DependencyProperty StageEventProperty =
         DependencyProperty.Register(nameof(StageEvent), typeof(TransitionEvent), typeof(PerformanceStage), new PropertyMetadata(TransitionEvent.Loaded));

    /// <summary>
    /// Gets or sets a value indicating whether playback is currently active.
    /// </summary>
    public bool Play
    {
        get => (bool)GetValue(PlayProperty);
        set => SetValue(PlayProperty, value);
    }

    /// <summary>
    /// Identifies the Play dependency property.
    /// </summary>
    /// <remarks>This field is used to register and reference the Play property with the Windows Presentation
    /// Foundation (WPF) property system. It is typically used when calling methods such as SetValue or GetValue on
    /// instances of PerformanceStage.</remarks>
    public static readonly DependencyProperty PlayProperty =
        DependencyProperty.Register(nameof(Play), typeof(bool), typeof(PerformanceStage), new PropertyMetadata(false));

    #region private

    private static StoryboardMapper GetStoryboardMapper(DependencyObject obj)
    {
        return (StoryboardMapper)obj.GetValue(StoryboardMapperProperty);
    }

    private static void SetStoryboardMapper(DependencyObject obj, StoryboardMapper value)
    {
        obj.SetValue(StoryboardMapperProperty, value);
    }

    private static readonly DependencyProperty StoryboardMapperProperty =
        DependencyProperty.RegisterAttached("StoryboardMapper", typeof(StoryboardMapper), typeof(PerformanceStage), new PropertyMetadata(null));

    private class StoryboardMapper : Dictionary<TransitionEvent, Storyboard> { }

    #endregion

    /// <summary>
    /// Gets the object to which this behavior is attached.
    /// </summary>
    /// <remarks>The associated object is typically set when the behavior is attached and cleared when it is
    /// detached. Access this property to interact with the object that the behavior is currently extending.</remarks>
    protected DependencyObject AssociatedObject { get; private set; } = default!;

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
            DataContextChangedEventManager.AddHandler(framework2, OnDataContextChanged);
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

        if (storyboardMaps.TryGetValue(StageEvent, out var storyboard) == false)
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

    internal void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (StageEvent == TransitionEvent.Loaded && sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.Loaded, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private void OnDataContextChanged(object? sender, EventArgs args)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.DataContextChanged, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private void OnMouseEnter(object? sender, MouseEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.MouseEnter, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private void OnMouseLeave(object? sender, MouseEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.MouseLeave, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private void OnGotFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.GotFocus, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject dependencyObject && GetStoryboardMapper(dependencyObject!) is StoryboardMapper mapper)
        {
            if (mapper.TryGetValue(TransitionEvent.LostFocus, out var storyboard))
            {
                storyboard.Begin();
            }
        }
    }

    private class DataContextChangedEventManager : WeakEventManager
    {
        protected override void StartListening(object source)
        {
            if (source is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContextChanged += OnDataContextChanged;
            }
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            base.DeliverEvent(sender, EventArgs.Empty);
        }

        protected override void StopListening(object source)
        {
            if (source is FrameworkElement frameworkElement)
            {
                frameworkElement.DataContextChanged -= OnDataContextChanged;
            }
        }

        public static void AddHandler(FrameworkElement source, EventHandler handler)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, (Delegate)(object)handler);
        }

        public static void RemoveHandler(FrameworkElement source, EventHandler handler)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = handler ?? throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, (Delegate)(object)handler);
        }

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
    }
}

