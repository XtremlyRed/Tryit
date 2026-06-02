using System.Diagnostics;
using System.Xml.Linq;

namespace Tryit;

/// <summary>
/// Represents the base contract for a named unit of work that can be executed within a Tryit context.
/// </summary>
/// <remarks>
/// An <see cref="Activity"/> defines a reusable execution step that participates in a larger workflow.
/// Derived types supply a stable display name through <see cref="Name"/> and implement the actual
/// asynchronous behavior in <see cref="RunAsync(IContext)"/>. Implementations can use the provided
/// <see cref="IContext"/> to exchange data, access services, or coordinate state across composed
/// activities.
/// </remarks>
public abstract class Activity
{
    /// <summary>
    /// Gets the display name of the activity.
    /// </summary>
    /// <value>
    /// A non-null string that identifies the activity in logs, diagnostics, or composed execution flows.
    /// </value>
    public abstract string Name { get; }

    /// <summary>
    /// Executes the activity asynchronously using the supplied execution context.
    /// </summary>
    /// <param name="context">
    /// The execution context that provides the data and services required by the activity while it runs.
    /// </param>
    /// <returns>
    /// A task that represents the lifetime of the activity execution.
    /// </returns>
    /// <remarks>
    /// Implementations should complete the returned task only when the activity has fully finished its
    /// work. Any exception thrown during execution will be propagated to the caller through the returned
    /// task.
    /// </remarks>
    public abstract Task RunAsync(IContext context);
}

/// <summary>
/// Provides convenience extension methods for registering delegate-based activities with a
/// <see cref="CompositeActivity"/>.
/// </summary>
/// <remarks>
/// These helpers wrap delegates in concrete <see cref="Activity"/> implementations so callers can add
/// lightweight inline behavior to a composite workflow without explicitly instantiating activity classes.
/// </remarks>
public static class ActivityExtensions
{
    /// <summary>
    /// Creates and adds an asynchronous delegate-based activity to the specified composite activity.
    /// </summary>
    /// <param name="compositeActivity">
    /// The composite activity that will receive the new child activity.
    /// </param>
    /// <param name="name">
    /// The logical name assigned to the generated activity.
    /// </param>
    /// <param name="funcTask">
    /// The asynchronous delegate to execute when the generated activity runs.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="compositeActivity"/>, <paramref name="name"/>, or
    /// <paramref name="funcTask"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The supplied delegate is wrapped in a <see cref="FuncTaskActivity"/> instance before being added
    /// to the composite activity.
    /// </remarks>
    public static void Add(this CompositeActivity compositeActivity, string name, Func<IContext, Task> funcTask)
    {
        _ = compositeActivity ?? throw new ArgumentNullException(nameof(compositeActivity));
        _ = name ?? throw new ArgumentNullException(nameof(name));
        _ = funcTask ?? throw new ArgumentNullException(nameof(funcTask));

        compositeActivity.Add(new FuncTaskActivity(name, funcTask));
    }

    /// <summary>
    /// Creates and adds a synchronous delegate-based activity to the specified composite activity.
    /// </summary>
    /// <param name="compositeActivity">
    /// The composite activity that will receive the new child activity.
    /// </param>
    /// <param name="name">
    /// The logical name assigned to the generated activity.
    /// </param>
    /// <param name="action">
    /// The synchronous delegate to execute within the supplied activity context.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="compositeActivity"/>, <paramref name="name"/>, or
    /// <paramref name="action"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    /// The supplied delegate is wrapped in an <see cref="ActionActivity"/> instance before being added to
    /// the composite activity.
    /// </remarks>
    public static void Add(this CompositeActivity compositeActivity, string name, Action<IContext> action)
    {
        _ = compositeActivity ?? throw new ArgumentNullException(nameof(compositeActivity));
        _ = name ?? throw new ArgumentNullException(nameof(name));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        compositeActivity.Add(new ActionActivity(name, action));
    }

    /// <summary>
    /// Adds one or more child activities to the composite activity.
    /// </summary>
    /// <param name="compositeActivity"></param>
    /// <param name="activities">
    /// The sequence of activities to add to the internal collection.
    /// </param>
    /// <remarks>
    /// Null entries in the supplied sequence are ignored. Because the backing collection is a
    /// <see cref="HashSet{T}"/>, duplicate activity references are not added more than once.
    /// </remarks>
    public static void Add(this CompositeActivity compositeActivity, params IEnumerable<Activity> activities)
    {
        _ = compositeActivity ?? throw new ArgumentNullException(nameof(compositeActivity));
        _ = activities ?? throw new ArgumentNullException(nameof(activities));

        foreach (Activity? item in activities)
        {
            if (item is not null)
            {
                compositeActivity.activities.Add(item);
            }
        }
    }
}
