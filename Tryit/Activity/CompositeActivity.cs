using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Represents an activity that groups multiple child activities and executes them as a single unit.
/// </summary>
/// <remarks>
/// A <see cref="CompositeActivity"/> implements the composite pattern for <see cref="Activity"/> objects.
/// It maintains an internal collection of child activities and runs them sequentially in the order they
/// are enumerated by the backing collection. This type is useful for building reusable workflows from
/// smaller execution steps.
/// </remarks>
public class CompositeActivity : Activity
{
    /// <summary>
    /// Stores the child activities that belong to this composite activity.
    /// </summary>
    /// <remarks>
    /// The field is marked internal so related types in the assembly can inspect or extend the composite
    /// behavior when necessary. The debugger display is hidden to reduce noise while inspecting instances.
    /// </remarks>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal readonly HashSet<Activity> activities = new HashSet<Activity>();

    /// <summary>
    /// Gets the display name of the composite activity.
    /// </summary>
    /// <value>
    /// A string that identifies this composite activity in diagnostics, logs, and execution flows.
    /// </value>
    public override string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeActivity"/> class with the specified name.
    /// </summary>
    /// <param name="name">The display name assigned to the composite activity.</param>
    public CompositeActivity(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    /// <summary>
    /// Executes each child activity asynchronously using the supplied execution context.
    /// </summary>
    /// <param name="context">
    /// The execution context passed to every child activity during the run.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous execution of all child activities.
    /// </returns>
    /// <remarks>
    /// Child activities are executed one at a time. If a child activity reference is <see langword="null"/>,
    /// it is skipped. If any child activity throws an exception, execution stops and the exception is
    /// propagated to the caller.
    /// </remarks>
    public override async
#if NETSTANDARD2_0 || NETCOREAPP3_1
    Task
#else
    ValueTask
#endif

    RunAsync(IContext context)
    {
        foreach (var activity in activities)
        {
            if (activity is not null)
            {
                await activity.RunAsync(context).ConfigureAwait(false);
            }
        }
    }
}
