using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace System.Threading;

/// <summary>
/// Provides extension methods for retrieving task awaiters from task completion sources and collections of
/// tasks. Supports awaiting individual tasks, arrays, and collections.
/// </summary>
public static class TaskExtensions
{
    /// <summary>
    /// Retrieves an awaiter for the task associated with the provided task completion source.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by the task.</typeparam>
    /// <param name="taskCompletionSource">Used to obtain the task's awaiter for asynchronous operations.</param>
    /// <returns>An awaiter that allows for awaiting the completion of the task.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided task completion source is null.</exception>
    public static TaskAwaiter<T> GetAwaiter<T>(this TaskCompletionSource<T> taskCompletionSource)
    {
        _ = taskCompletionSource ?? throw new ArgumentNullException(nameof(taskCompletionSource));

        return taskCompletionSource.Task.GetAwaiter();
    }

    /// <summary>
    /// Retrieves a task awaiter for an array of task completion sources, allowing for asynchronous waiting on their
    /// completion.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by each task completion source.</typeparam>
    /// <param name="taskCompletionSources">An array of task completion sources to be awaited for their completion.</param>
    /// <returns>Returns a task awaiter for an array of results from the completed tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided array of task completion sources is null.</exception>
    public static TaskAwaiter<T[]> GetAwaiter<T>(this TaskCompletionSource<T>[] taskCompletionSources)
    {
        _ = taskCompletionSources ?? throw new ArgumentNullException(nameof(taskCompletionSources));

        return Task.WhenAll(taskCompletionSources.Select(i => i.Task)).GetAwaiter();
    }

    /// <summary>
    /// Retrieves a task awaiter for an array of results from a collection of task completion sources.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by each task completion source.</typeparam>
    /// <param name="taskCompletionSources">A collection of task completion sources from which to await the completion of tasks.</param>
    /// <returns>An awaiter for a task that completes when all tasks in the collection have completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection of task completion sources is null.</exception>
    public static TaskAwaiter<T[]> GetAwaiter<T>(this IEnumerable<TaskCompletionSource<T>> taskCompletionSources)
    {
        _ = taskCompletionSources ?? throw new ArgumentNullException(nameof(taskCompletionSources));

        return Task.WhenAll(taskCompletionSources.Select(i => i.Task)).GetAwaiter();
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    public static TaskAwaiter GetAwaiter(this TimeSpan tasks)
    {
        return Task.Delay(tasks).GetAwaiter();
    }

    /// <summary>
    /// Retrieves a task awaiter for a collection of tasks, allowing for asynchronous waiting on their completion.
    /// </summary>
    /// <param name="tasks">The collection of tasks to be awaited for completion.</param>
    /// <returns>An awaiter that can be used to asynchronously wait for all tasks in the collection to complete.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the provided collection of tasks is null.</exception>
    public static TaskAwaiter GetAwaiter(this IEnumerable<Task> tasks)
    {
        _ = tasks ?? throw new ArgumentNullException(nameof(tasks));

        return Task.WhenAll(tasks).GetAwaiter();
    }

    /// <summary>
    /// Retrieves a task awaiter for an array of results from a collection of tasks.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by each task in the collection.</typeparam>
    /// <param name="tasks">A collection of tasks whose results will be awaited and combined into an array.</param>
    /// <returns>An awaiter that allows for asynchronous waiting on the completion of all tasks.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection of tasks is null.</exception>
    public static TaskAwaiter<T[]> GetAwaiter<T>(this IEnumerable<Task<T>> tasks)
    {
        _ = tasks ?? throw new ArgumentNullException(nameof(tasks));

        return Task.WhenAll(tasks).GetAwaiter();
    }

    /// <summary>
    /// Executes a collection of asynchronous functions and returns a task awaiter for their completion.
    /// </summary>
    /// <typeparam name="T">Specifies the type of task that the functions in the collection will return upon execution.</typeparam>
    /// <param name="taskFuncs">Represents a collection of functions that return tasks to be executed asynchronously.</param>
    /// <returns>Provides a task awaiter that allows waiting for all the tasks to complete.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the collection of functions is null.</exception>
    public static TaskAwaiter GetAwaiter<T>(this IEnumerable<Func<T>> taskFuncs)
        where T : Task
    {
        _ = taskFuncs ?? throw new ArgumentNullException(nameof(taskFuncs));

        var awaiters = new List<Task>();

        foreach (var taskFunc in taskFuncs)
        {
            if (taskFunc is null)
            {
                continue;
            }

            var opt = new TaskOpt<T>(taskFunc);

            awaiters.Add(opt.TaskCompletion.Task);

            ThreadPool.QueueUserWorkItem(
                static async t =>
                {
                    try
                    {
                        await ((TaskOpt<T>)t!).Func();

                        ((TaskOpt<T>)t).TaskCompletion.TrySetResult(true);
                    }
                    catch (Exception ex)
                    {
                        ((TaskOpt<T>)t!).TaskCompletion.TrySetException(ex);
                    }
                },
                opt
            );
        }

        return Task.WhenAll(awaiters).GetAwaiter();
    }

    /// <summary>
    /// Represents an option for a task that can be executed, encapsulating a function that returns a task.
    /// </summary>
    /// <typeparam name="T">Specifies the type of task that will be created and executed.</typeparam>
    /// <param name="Func">Defines the function that produces the task to be executed.</param>
    record TaskOpt<T>(Func<T> Func)
        where T : Task
    {
        public TaskCompletionSource<bool> TaskCompletion = new TaskCompletionSource<bool>();
    }
}
