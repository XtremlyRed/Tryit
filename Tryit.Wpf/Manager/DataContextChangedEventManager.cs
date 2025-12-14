using System.Windows;

namespace Tryit.Wpf;

internal class DataContextChangedEventManager : WeakEventManager
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