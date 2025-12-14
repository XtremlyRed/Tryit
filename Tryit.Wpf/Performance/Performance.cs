using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace Tryit.Wpf;

/// <summary>
/// Provides attached properties and methods for associating and retrieving performance stage information with
/// dependency objects.
/// </summary>
/// <remarks>The Performance class enables the association of performance tracking data with UI elements or other
/// dependency objects through attached properties. This facilitates performance analysis and monitoring scenarios in
/// applications that utilize the dependency property system.</remarks>
public static class Performance
{

    /// <summary>
    /// Retrieves the collection of performance stages associated with the specified dependency object. If no collection
    /// exists, a new one is created and associated with the object.
    /// </summary>
    /// <remarks>This method ensures that each dependency object has a unique <see cref="PerformanceStages"/>
    /// instance. Subsequent calls with the same object will return the same instance.</remarks>
    /// <param name="dependencyObject">The dependency object for which to retrieve the associated performance stages. Cannot be null.</param>
    /// <returns>A <see cref="PerformanceStages"/> instance associated with the specified dependency object.</returns>
    public static PerformanceStages GetStages(DependencyObject dependencyObject)
    {
        if (dependencyObject.GetValue(StagesProperty) is not PerformanceStages stages)
        {
            dependencyObject.SetValue(StagesProperty, stages = new PerformanceStages(dependencyObject));
        }
        return stages;

    }

    /// <summary>
    /// Identifies the ShadowStages attached dependency property, which stores the performance stages associated with a
    /// UI element.
    /// </summary>
    /// <remarks>This field is used when adding or retrieving the ShadowStages property value on a
    /// DependencyObject, typically via GetValue and SetValue methods. The property enables associating a
    /// PerformanceStages value with elements in the visual tree for performance tracking or analysis.</remarks>
    public static readonly DependencyProperty StagesProperty = DependencyProperty.RegisterAttached("ShadowStages", typeof(PerformanceStages), typeof(Performance), new PropertyMetadata(null));

}

/// <summary>
/// Represents a collection of performance stages that can be associated with a DependencyObject, enabling stage-based
/// handling of lifecycle events such as loading.
/// </summary>
/// <remarks>PerformanceStages is typically used to manage and execute a sequence of PerformanceStage instances in
/// response to the lifecycle events of an associated FrameworkElement. When constructed with a FrameworkElement, the
/// collection automatically attaches to the Loaded event, allowing each contained stage to be initialized and invoked
/// at the appropriate time. This class extends FreezableCollection, providing collection semantics and support for XAML
/// scenarios.</remarks>
public class PerformanceStages : FreezableCollection<PerformanceStage>, ICollection<PerformanceStage>, IList
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly DependencyObject associatedObject;

    /// <summary>
    /// Initializes a new instance of the PerformanceStages class and attaches to the Loaded event of the specified
    /// associated object, if it is a FrameworkElement.
    /// </summary>
    /// <remarks>This constructor enables the PerformanceStages instance to respond to the lifecycle events of
    /// the associated FrameworkElement. If the provided object is not a FrameworkElement, no event handlers are
    /// attached.</remarks>
    /// <param name="associatedObject">The DependencyObject to associate with this instance. If the object is a FrameworkElement, the Loaded event will
    /// be monitored. Cannot be null.</param>
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