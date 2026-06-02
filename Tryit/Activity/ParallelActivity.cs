using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Represents a composite activity that executes its child activities concurrently.
/// </summary>
/// <remarks>
/// Unlike <see cref="CompositeActivity"/>, which executes child activities sequentially,
/// <see cref="ParallelActivity"/> schedules each contained activity on the thread pool and waits until
/// every scheduled operation has completed. This type is useful when multiple independent activities can
/// run at the same time while sharing the same execution context.
/// </remarks>
public class ParallelActivity : CompositeActivity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ParallelActivity"/> class with the specified name.
    /// </summary>
    /// <param name="name">The display name assigned to the parallel activity.</param>
    public ParallelActivity(string name)
        : base(name) { }

    /// <summary>
    /// Executes all child activities in parallel using the supplied execution context.
    /// </summary>
    /// <param name="context">The execution context passed to each child activity.</param>
    /// <returns>A task that represents the completion of all scheduled child activities.</returns>
    /// <remarks>
    /// Each child activity is wrapped in an internal completion map and queued to the thread pool. The
    /// returned task completes only after all child activities have finished. If one or more child
    /// activities fail, their exceptions are captured and propagated through the awaited completion objects.
    /// </remarks>
    public override async Task RunAsync(IContext context)
    {
        var aMaps = new AMap[activities.Count];

        var index = 0;

        foreach (var item in activities)
        {
            AMap aMap = aMaps[index++] = new AMap(item, context);

            ThreadPool.QueueUserWorkItem(static async c => await ((AMap)c!).RunAsync().ConfigureAwait(false), aMap);
        }

        await aMaps;
    }

    /// <summary>
    /// Bridges a queued activity execution with a task-based completion source.
    /// </summary>
    /// <remarks>
    /// Each instance stores the activity to execute together with the shared execution context. When the
    /// queued work completes, the inherited <see cref="TaskCompletionSource{TResult}"/> is completed with
    /// either a successful result or an exception.
    /// </remarks>
    class AMap(Activity Activity, IContext Context) : TaskCompletionSource<bool>
    {
        /// <summary>
        /// Executes the stored activity and completes the underlying task completion source.
        /// </summary>
        /// <returns>A task that represents the asynchronous execution of the mapped activity.</returns>
        /// <remarks>
        /// On successful completion, the task completion source is marked with a <see langword="true"/>
        /// result. If the activity throws an exception, that exception is captured and assigned to the task
        /// completion source so it can be observed by the caller.
        /// </remarks>
        public async Task RunAsync()
        {
            try
            {
                await Activity.RunAsync(Context).ConfigureAwait(false);

                TrySetResult(true);
            }
            catch (Exception ex)
            {
                TrySetException(ex);
            }
        }
    }
}
