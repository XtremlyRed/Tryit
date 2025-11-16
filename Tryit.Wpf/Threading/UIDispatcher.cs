using System.Runtime.ExceptionServices;

namespace Tryit.Wpf.Threading;

/// <summary>
/// Provides utility methods for creating and managing background UI threads with their own WPF Dispatcher instances.
/// </summary>
/// <remarks>The UIDispatcher class enables the creation of dedicated background threads that host their own
/// Dispatcher, allowing for offloading UI-related work or message loops to separate threads. This can be useful for
/// scenarios where isolated UI processing is required, such as background rendering, dialog management, or running
/// components that require a message loop outside the main UI thread. All methods are static and thread-safe. The
/// created threads use single-threaded apartment (STA) state and are marked as background threads.</remarks>
public static class UIDispatcher
{
    /// <summary>
    /// Starts a new background thread with its own Dispatcher and returns an awaitable operation for accessing the
    /// created Dispatcher instance.
    /// </summary>
    /// <remarks>The created Dispatcher runs on a dedicated STA thread and can be used to schedule work on
    /// that thread. Awaiting the returned operation ensures that the Dispatcher is fully initialized before use.
    /// Unhandled exceptions on the background Dispatcher thread are propagated to the calling thread.</remarks>
    /// <param name="name">The name to assign to the background thread. If null, a default name is used.</param>
    /// <returns>A DispatcherAsyncOperation that completes when the new Dispatcher is created. The result contains the Dispatcher
    /// associated with the new background thread.</returns>
    public static DispatcherAsyncOperation<Dispatcher> RunNewAsync(string? name = null)
    {
        DispatcherAsyncOperation<Dispatcher> awaiter = DispatcherAsyncOperation<Dispatcher>.Create(out Action<Dispatcher, Exception>? reportResult);

        Dispatcher originDispatcher = Dispatcher.CurrentDispatcher;

        Thread thread = new(() =>
        {
            try
            {
                Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

                reportResult(dispatcher, null!);
            }
            catch (Exception ex)
            {
                reportResult(null!, ex);
            }

            try
            {
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                originDispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        })
        {
            Name = name ?? "BackgroundUI",
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return awaiter;
    }

    /// <summary>
    /// Creates and starts a new background thread with its own WPF Dispatcher, returning the Dispatcher instance for use in
    /// cross-thread operations.
    /// </summary>
    /// <remarks>The returned Dispatcher runs on a dedicated single-threaded apartment (STA) background thread. This
    /// can be used to marshal work to a separate UI thread in WPF applications. The method blocks until the Dispatcher is
    /// fully initialized and ready to process messages. Any exception thrown during Dispatcher initialization is rethrown
    /// on the calling thread.</remarks>
    /// <param name="name">The name to assign to the new background thread. If null, a default name is used.</param>
    /// <returns>The Dispatcher associated with the newly created background thread.</returns>
    public static Dispatcher RunNew(string? name = null)
    {
        AutoResetEvent? resetEvent = new(false);

        Dispatcher originDispatcher = Dispatcher.CurrentDispatcher;
        Exception innerException = null!;
        Dispatcher dispatcher = null!;

        Thread thread = new(() =>
        {
            try
            {
                dispatcher = Dispatcher.CurrentDispatcher;

                SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(dispatcher));

                resetEvent.Set();
            }
            catch (Exception ex)
            {
                innerException = ex;
                resetEvent.Set();
            }

            try
            {
                Dispatcher.Run();
            }
            catch (Exception ex)
            {
                originDispatcher.InvokeAsync(() => ExceptionDispatchInfo.Capture(ex).Throw());
            }
        })
        {
            Name = name ?? "BackgroundUI",
            IsBackground = true,
        };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        resetEvent.WaitOne();
        resetEvent.Dispose();
        resetEvent = null;
        if (innerException != null)
        {
            ExceptionDispatchInfo.Capture(innerException).Throw();
        }
        return dispatcher;
    }
}
