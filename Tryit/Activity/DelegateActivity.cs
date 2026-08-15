using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Represents an <see cref="Activity"/> whose execution logic is provided by an asynchronous delegate.
/// </summary>
/// <remarks>
/// This type adapts a <see cref="Func{T, TResult}"/> accepting an <see cref="IContext"/> into the
/// <see cref="Activity"/> abstraction, allowing inline asynchronous behavior to participate in a larger
/// activity pipeline without creating a dedicated derived class.
/// </remarks>
public class FuncTaskActivity : Activity
{
    /// <summary>
    /// Stores the asynchronous delegate invoked when the activity runs.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Func<IContext, Task> funcTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="FuncTaskActivity"/> class.
    /// </summary>
    /// <param name="name">The display name assigned to the activity.</param>
    /// <param name="funcTask">The asynchronous delegate to execute when the activity runs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="funcTask"/> is <see langword="null"/>.</exception>
    public FuncTaskActivity(string name, Func<IContext, Task> funcTask)
    {
        Name = name;
        this.funcTask = funcTask ?? throw new ArgumentNullException(nameof(funcTask));
    }

    /// <summary>
    /// Gets the display name of the activity.
    /// </summary>
    /// <value>
    /// A string that identifies this activity in workflow definitions, diagnostics, and logs.
    /// </value>
    public override string Name { get; }

    /// <summary>
    /// Executes the stored asynchronous delegate using the supplied execution context.
    /// </summary>
    /// <param name="context">The execution context passed to the delegate.</param>
    /// <returns>A task that represents the asynchronous delegate execution.</returns>
    /// <remarks>
    /// Any exception produced by the delegate is propagated through the returned task.
    /// </remarks>
    public override async
#if NETSTANDARD2_0 || NETCOREAPP3_1
    Task
#else
    ValueTask
#endif
    RunAsync(IContext context)
    {
        await funcTask(context).ConfigureAwait(false);
    }
}

/// <summary>
/// Represents an <see cref="Activity"/> whose execution logic is provided by a synchronous delegate.
/// </summary>
/// <remarks>
/// This type adapts an <see cref="Action{T}"/> accepting an <see cref="IContext"/> into the
/// <see cref="Activity"/> abstraction, making it easy to compose lightweight synchronous steps into an
/// activity workflow.
/// </remarks>
public class ActionActivity : Activity
{
    /// <summary>
    /// Stores the synchronous delegate invoked when the activity runs.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Action<IContext> action;

    /// <summary>
    /// Initializes a new instance of the <see cref="ActionActivity"/> class.
    /// </summary>
    /// <param name="name">The display name assigned to the activity.</param>
    /// <param name="action">The synchronous delegate to execute when the activity runs.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public ActionActivity(string name, Action<IContext> action)
    {
        Name = name;
        this.action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Gets the display name of the activity.
    /// </summary>
    /// <value>
    /// A string that identifies this activity in workflow definitions, diagnostics, and logs.
    /// </value>
    public override string Name { get; }

    /// <summary>
    /// Executes the stored synchronous delegate using the supplied execution context.
    /// </summary>
    /// <param name="context">The execution context passed to the delegate.</param>
    /// <returns>A completed task after the synchronous delegate finishes execution.</returns>
    /// <remarks>
    /// Exceptions thrown by the delegate are propagated directly to the caller before the completed task is returned.
    /// </remarks>
    public override
#if NETSTANDARD2_0 || NETCOREAPP3_1
    Task
#else
    ValueTask
#endif

    RunAsync(IContext context)
    {
        action(context);

#if NETSTANDARD2_0 || NETCOREAPP3_1
        return Task.CompletedTask;
#else
        return ValueTask.CompletedTask;

#endif
    }
}
