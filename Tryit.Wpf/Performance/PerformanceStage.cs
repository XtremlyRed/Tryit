using System.Collections;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Tryit.Wpf;

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

    public TransitionEvent StageEvent
    {
        get => (TransitionEvent)GetValue(StageEventProperty);
        set => SetValue(StageEventProperty, value);
    }

    public static readonly DependencyProperty StageEventProperty =
         DependencyProperty.Register(nameof(StageEvent), typeof(TransitionEvent), typeof(PerformanceStage), new PropertyMetadata(TransitionEvent.Loaded));

    public bool Play
    {
        get => (bool)GetValue(PlayProperty);
        set => SetValue(PlayProperty, value);
    }

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

    protected DependencyObject AssociatedObject { get; private set; } = default!;

    internal void OnInitialize(DependencyObject dependencyObject)
    {
        if ((AssociatedObject = dependencyObject) is not UIElement element)
        {
            return;
        }

        if (element.RenderTransform is not TransformGroup transformGroup)
        {
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(element.RenderTransform);
            element.RenderTransform = transformGroup;
        }

        _ = transformGroup.TryAdd<TranslateTransform>();
        _ = transformGroup.TryAdd<ScaleTransform>();
        _ = transformGroup.TryAdd<RotateTransform>();
        _ = transformGroup.TryAdd<SkewTransform>();

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
            if (this[i].CreateAnimation(AssociatedObject) is AnimationTimeline animationTimeline)
            {
                storyboard.Children.Add(animationTimeline);
            }
        }

        if (StageEvent != TransitionEvent.Loaded && element is FrameworkElement frameworkElement)
        {
            WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(frameworkElement, nameof(FrameworkElement.Loaded), OnLoaded);
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
}

