using System.Collections;
using System.Windows;

namespace Tryit.Wpf;

public static class Performance
{
    public static PerformanceStages GetStages(DependencyObject dependencyObject)
    {
        if (dependencyObject.GetValue(StagesProperty) is not PerformanceStages stages)
        {
            dependencyObject.SetValue(StagesProperty, stages = new PerformanceStages(dependencyObject));
        }
        return stages;

    }

    public static readonly DependencyProperty StagesProperty = DependencyProperty.RegisterAttached("ShadowStages", typeof(PerformanceStages), typeof(Performance), new PropertyMetadata(null));

}

public class PerformanceStages : FreezableCollection<PerformanceStage>, ICollection<PerformanceStage>, IList
{
    private readonly DependencyObject associatedObject;

    public PerformanceStages(DependencyObject associatedObject)
    {
        if ((this.associatedObject = associatedObject) is FrameworkElement element)
        {
            WeakEventManager<FrameworkElement, RoutedEventArgs>.AddHandler(element, nameof(FrameworkElement.Loaded), OnLoaded);
        }
    }

    int IList.Add(object? item)
    {
        if (item is not PerformanceStage stage)
        {
            return 0;
        }

        if (this.Any(x => x.StageEvent == stage.StageEvent))
        {
            throw new ArgumentException($"exist performance stage : {stage.StageEvent}");
        }

        base.Add(stage);

        return 1;

    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (associatedObject is UIElement element)
        {
            if (element is FrameworkElement felement)
            {
                WeakEventManager<FrameworkElement, RoutedEventArgs>.RemoveHandler(felement, nameof(FrameworkElement.Loaded), OnLoaded);
            }
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            this[i].OnInitialize(associatedObject);
        }

        for (int i = Count - 1; i >= 0; i--)
        {
            this[i].OnLoaded(sender, e);
        }
    }
}