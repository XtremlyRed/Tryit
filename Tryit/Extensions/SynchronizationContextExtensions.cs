using System.Runtime.InteropServices;

namespace System.Threading;

/// <summary>
/// Provides extension methods for executing actions and functions on a specified synchronization context, both
/// synchronously and asynchronously. Handles exceptions and returns results as needed.
/// </summary>
public static class SynchronizationContextExtensions
{
    record PostMapBase<T>
    {
        private TaskCompletionSource<T> TaskCompletionSource = new();

        public async Task<T> WaitAsync()
        {
            var result = await TaskCompletionSource.Task;

            return result;
        }

        public void SetResult(T value)
        {
            TaskCompletionSource.SetResult(value);
        }

        public void SetException(Exception exception)
        {
            TaskCompletionSource.SetException(exception);
        }
    }

    record PostFuncMap<T>(Func<T> Action) : PostMapBase<T> { }

    record PostFuncMapAsync<T>(Func<Task<T>> Action) : PostMapBase<T> { }

    record PostFuncMapAsync(Func<Task> Action) : PostMapBase<bool> { }

    record PostActionMap(Action Action) : PostMapBase<bool> { }

    /// <summary>
    /// Executes a specified action on the thread associated with the provided synchronization context.
    /// </summary>
    /// <param name="context">The synchronization context that determines the thread on which the action will be executed.</param>
    /// <param name="action">The action to be executed on the specified synchronization context.</param>
    /// <exception cref="ArgumentNullException">Thrown when either the synchronization context or the action is null.</exception>
    public static void Post(this SynchronizationContext context, Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        context.Post(o => ((Action)o!)!(), action);
    }

    /// <summary>
    /// Executes an action on the specified synchronization context asynchronously.
    /// </summary>
    /// <param name="context">The synchronization context where the action will be executed.</param>
    /// <param name="action">The action to be performed on the synchronization context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the action or synchronization context is null.</exception>
    public static async Task PostAsync(this SynchronizationContext context, Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostActionMap(action);

        context.Post(
            static o =>
            {
                if (o is PostActionMap postMap)
                {
                    try
                    {
                        postMap.Action();
                        postMap.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap.WaitAsync();
    }

    /// <summary>
    /// Executes an asynchronous action on a specified synchronization context.
    /// </summary>
    /// <param name="context">The synchronization context where the action will be executed.</param>
    /// <param name="action">The asynchronous function to be executed within the context.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the action or context is null.</exception>
    public static async Task PostAsync(this SynchronizationContext context, Func<Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMapAsync(action);

        context.Post(
            static o =>
            {
                if (o is PostFuncMapAsync postMap)
                {
                    try
                    {
                        postMap.Action();
                        postMap.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        await postMap.WaitAsync();
    }

    /// <summary>
    /// Executes an asynchronous function on a specified synchronization context and returns the result.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned by the asynchronous function.</typeparam>
    /// <param name="context">Specifies the synchronization context in which the asynchronous function will be executed.</param>
    /// <param name="action">Defines the asynchronous function to be executed within the specified context.</param>
    /// <returns>Returns the result of the asynchronous function once it completes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the action or context parameter is null.</exception>
    public static async Task<T> PostAsync<T>(this SynchronizationContext context, Func<Task<T>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMapAsync<T>(action);

        context.Post(
            static async o =>
            {
                if (o is PostFuncMapAsync<T> postMap)
                {
                    try
                    {
                        var result = await postMap.Action();
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        return await postMap.WaitAsync();
    }

    /// <summary>
    /// Executes a specified function on a given synchronization context asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by the function being executed.</typeparam>
    /// <param name="context">Specifies the synchronization context in which the function will be executed.</param>
    /// <param name="action">Defines the function to be executed within the specified synchronization context.</param>
    /// <returns>Returns the result of the executed function once it completes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the action or context parameter is null.</exception>
    public static async Task<T> PostAsync<T>(this SynchronizationContext context, Func<T> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));
        _ = context ?? throw new ArgumentNullException(nameof(context));

        var postMap = new PostFuncMap<T>(action);

        context.Post(
            static o =>
            {
                if (o is PostFuncMap<T> postMap)
                {
                    try
                    {
                        var result = postMap.Action();
                        postMap.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        postMap.SetException(ex);
                    }
                }
            },
            postMap
        );

        return await postMap.WaitAsync();
    }
}
