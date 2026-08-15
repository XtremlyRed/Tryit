namespace System.Threading;

/// <summary>
/// A small set of convenience extension helpers that make queuing work items to the
/// <see cref="ThreadPool"/> simpler by accepting strongly-typed callbacks or returning
/// <see cref="TaskCompletionSource{TResult}"/> instances for asynchronous coordination.
/// These helpers reduce the boilerplate of creating state objects and managing Task completion
/// when invoking work on the thread pool.
/// </summary>
public static class ThreadPoolExtensions
{
    /// <summary>
    /// Queues the specified <see cref="Action"/> to the thread pool.
    /// This overload avoids the caller needing to wrap the action in an object state parameter.
    /// </summary>
    /// <param name="action">The action to execute on a thread-pool thread. Cannot be null.</param>
    public static void QueueUserWorkItem(Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        ThreadPool.QueueUserWorkItem(o => ((Action)o!)!(), action);
    }

    /// <summary>
    /// Queues an <see cref="Action{T}"/> with a single strongly-typed state parameter to the thread pool.
    /// The callback receives <paramref name="targetValue"/> when executed.
    /// </summary>
    /// <typeparam name="T">Type of the state value passed to the callback.</typeparam>
    /// <param name="targetValue">State value to pass to the callback.</param>
    /// <param name="callback">The callback to execute. Cannot be null.</param>
    public static void QueueUserWorkItem<T>(T targetValue, Action<T> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        ThreadPool.QueueUserWorkItem(
            static o =>
            {
                ValueTuple<T, Action<T>> vt = (ValueTuple<T, Action<T>>)o!;

                vt.Item2.Invoke(vt.Item1);
            },
            ValueTuple.Create(targetValue, callback)
        );
    }

    /// <summary>
    /// Queues an <see cref="Action{T1,T2}"/> with two state values to the thread pool.
    /// The provided callback will be invoked with the supplied state values on a thread-pool thread.
    /// </summary>
    /// <typeparam name="T1">Type of the first state value.</typeparam>
    /// <typeparam name="T2">Type of the second state value.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="callback">The callback to execute. Cannot be null.</param>
    public static void QueueUserWorkItem<T1, T2>(T1 targetValue1, T2 targetValue2, Action<T1, T2> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T1, T2, Action<T1, T2>> vt)
                {
                    vt.Item3.Invoke(vt.Item1, vt.Item2);
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, callback)
        );
    }

    /// <summary>
    /// Queues an <see cref="Action{T1,T2,T3}"/> with three state values to the thread pool.
    /// </summary>
    /// <typeparam name="T1">Type of the first state value.</typeparam>
    /// <typeparam name="T2">Type of the second state value.</typeparam>
    /// <typeparam name="T3">Type of the third state value.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="targetValue3">Third state value passed to the callback.</param>
    /// <param name="callback">The callback to execute. Cannot be null.</param>
    public static void QueueUserWorkItem<T1, T2, T3>(T1 targetValue1, T2 targetValue2, T3 targetValue3, Action<T1, T2, T3> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T1, T2, T3, Action<T1, T2, T3>> vt)
                {
                    vt.Item4.Invoke(vt.Item1, vt.Item2, vt.Item3);
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, targetValue3, callback)
        );
    }

    /// <summary>
    /// Queues an asynchronous <see cref="Func{Task}"/> to the thread pool and returns a
    /// <see cref="TaskCompletionSource{Boolean}"/> that completes when the queued delegate finishes.
    /// Any exception thrown by the delegate will be propagated to the returned TCS as an exception.
    /// </summary>
    /// <param name="action">Asynchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that signals completion or failure of the invoked action.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync(Func<Task> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is not ValueTuple<Func<Task>, TaskCompletionSource<bool>> vt)
                {
                    return;
                }

                try
                {
                    await vt.Item1();
                    vt.Item2.SetResult(true);
                }
                catch (Exception ex)
                {
                    vt.Item2.SetException(ex);
                }
            },
            ValueTuple.Create(action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous callback with a single state parameter and returns a TCS that completes
    /// when the callback finishes or faults when it throws.
    /// </summary>
    /// <typeparam name="T">Type of the state parameter passed to the callback.</typeparam>
    /// <param name="targetValue">State value to pass to the callback.</param>
    /// <param name="callback">Asynchronous callback to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource used to observe completion of the callback.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync<T>(T targetValue, Func<T, Task> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            static async o =>
            {
                if (o is ValueTuple<T, Func<T, Task>, TaskCompletionSource<bool>> vt)
                {
                    try
                    {
                        await vt.Item2(vt.Item1);
                        vt.Item3.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        vt.Item3.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous callback with two state parameters and returns a TaskCompletionSource
    /// that completes when the callback finishes.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="callback">The asynchronous callback to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource observing the completion of the callback.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync<T1, T2>(T1 targetValue1, T2 targetValue2, Func<T1, T2, Task> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<T1, T2, Func<T1, T2, Task>, TaskCompletionSource<bool>> vt)
                {
                    try
                    {
                        await vt.Item3(vt.Item1, vt.Item2);
                        vt.Item4.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        vt.Item4.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous callback with three state parameters and returns a TaskCompletionSource
    /// that completes when the callback finishes.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <typeparam name="T3">Type of the third state parameter.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="targetValue3">Third state value passed to the callback.</param>
    /// <param name="callback">The asynchronous callback to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource observing the completion of the callback.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync<T1, T2, T3>(T1 targetValue1, T2 targetValue2, T3 targetValue3, Func<T1, T2, T3, Task> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<T1, T2, T3, Func<T1, T2, T3, Task>, TaskCompletionSource<bool>> vt)
                {
                    try
                    {
                        await vt.Item4(vt.Item1, vt.Item2, vt.Item3);
                        vt.Item5.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        vt.Item5.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, targetValue3, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous function that returns a result, and returns a TaskCompletionSource which
    /// will be completed with the function's return value when execution finishes.
    /// </summary>
    /// <typeparam name="TResult">Type of the result produced by the function.</typeparam>
    /// <param name="action">Function to execute on the thread pool. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function's result or faulted on exception.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<TResult>(Func<TResult> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is not ValueTuple<Func<TResult>, TaskCompletionSource<TResult>> vt)
                {
                    return;
                }

                try
                {
                    TResult result = vt.Item1();
                    vt.Item2.SetResult(result);
                }
                catch (Exception ex)
                {
                    vt.Item2.SetException(ex);
                }
            },
            ValueTuple.Create(action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous function with a single strongly-typed state parameter to the thread pool
    /// and returns a <see cref="TaskCompletionSource{TResult}"/> that will be completed with the
    /// function's result when the callback finishes. Exceptions thrown by the callback are captured
    /// and set on the TaskCompletionSource.
    /// </summary>
    /// <typeparam name="T1">Type of the state parameter passed to the callback.</typeparam>
    /// <typeparam name="TResult">Type of the result returned by the callback.</typeparam>
    /// <param name="targetValue1">State value passed to the callback.</param>
    /// <param name="callback">Synchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function result or faulted if the function throws.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, TResult>(T1 targetValue1, Func<T1, TResult> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T1, Func<T1, TResult>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        vt.Item3.SetResult(vt.Item2(vt.Item1));
                    }
                    catch (Exception ex)
                    {
                        vt.Item3.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue1, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous function with two strongly-typed state parameters to the thread pool
    /// and returns a <see cref="TaskCompletionSource{TResult}"/> that will complete with the
    /// function result when the callback finishes. Any exception thrown will be propagated to the returned TCS.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result returned by the callback.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="callback">Synchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function result or faulted if the function throws.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, T2, TResult>(T1 targetValue1, T2 targetValue2, Func<T1, T2, TResult> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T1, T2, Func<T1, T2, TResult>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        vt.Item4.SetResult(vt.Item3(vt.Item1, vt.Item2));
                    }
                    catch (Exception ex)
                    {
                        vt.Item4.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous function with three strongly-typed state parameters to the thread pool
    /// and returns a <see cref="TaskCompletionSource{TResult}"/> that will be completed with the
    /// function's result when execution finishes. Exceptions are captured and set on the returned TCS.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <typeparam name="T3">Type of the third state parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result returned by the callback.</typeparam>
    /// <param name="targetValue1">First state value passed to the callback.</param>
    /// <param name="targetValue2">Second state value passed to the callback.</param>
    /// <param name="targetValue3">Third state value passed to the callback.</param>
    /// <param name="callback">Synchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function result or faulted if the function throws.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, T2, T3, TResult>(T1 targetValue1, T2 targetValue2, T3 targetValue3, Func<T1, T2, T3, TResult> callback)
    {
        _ = callback ?? throw new ArgumentNullException(nameof(callback));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T1, T2, T3, Func<T1, T2, T3, TResult>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        vt.Item5.SetResult(vt.Item4(vt.Item1, vt.Item2, vt.Item3));
                    }
                    catch (Exception ex)
                    {
                        vt.Item5.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(targetValue1, targetValue2, targetValue3, callback, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous <see cref="Action"/> for execution on the thread pool and returns a
    /// <see cref="TaskCompletionSource{Boolean}"/> that completes when the action has finished.
    /// Exceptions thrown by the action are propagated to the returned TCS.
    /// </summary>
    /// <param name="action">Synchronous action to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be set to true on successful completion or faulted on exception.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync(Action action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<Action, TaskCompletionSource<bool>> vt)
                {
                    try
                    {
                        vt.Item1();
                        vt.Item2.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        vt.Item2.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues a synchronous <see cref="Action{T}"/> with a state parameter to the thread pool and
    /// returns a <see cref="TaskCompletionSource{Boolean}"/> that completes when the action finishes.
    /// Exceptions thrown by the action are captured and set on the returned TCS.
    /// </summary>
    /// <typeparam name="T">Type of the state parameter.</typeparam>
    /// <param name="parameter">State value passed to the action.</param>
    /// <param name="action">Action to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be set to true on success or faulted on exception.</returns>
    public static TaskCompletionSource<bool> QueueUserWorkItemAsync<T>(T parameter, Action<T> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<bool> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            o =>
            {
                if (o is ValueTuple<T, Action<T>, TaskCompletionSource<bool>> vt)
                {
                    try
                    {
                        vt.Item2(vt.Item1);
                        vt.Item3.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        vt.Item3.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(parameter, action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous function that returns a result and returns a <see cref="TaskCompletionSource{T}"/>
    /// which will be completed with the function's result when execution finishes. Exceptions are propagated
    /// to the returned TCS.
    /// </summary>
    /// <typeparam name="T">Type of the result produced by the asynchronous function.</typeparam>
    /// <param name="action">Asynchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function's result or faulted if it throws.</returns>
    public static TaskCompletionSource<T> QueueUserWorkItemAsync<T>(Func<Task<T>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<T> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<Func<Task<T>>, TaskCompletionSource<T>> vt)
                {
                    try
                    {
                        vt.Item2.SetResult(await vt.Item1());
                    }
                    catch (Exception ex)
                    {
                        vt.Item2.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous function with a single state parameter and returns a <see cref="TaskCompletionSource{TResult}"/>
    /// that will be completed with the function's result when execution finishes. Exceptions are captured and
    /// propagated to the returned TCS.
    /// </summary>
    /// <typeparam name="T1">Type of the state parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result returned by the asynchronous function.</typeparam>
    /// <param name="t1">State value passed to the asynchronous function.</param>
    /// <param name="action">Asynchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that completes with the function result or faults on exception.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, TResult>(T1 t1, Func<T1, Task<TResult>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<T1, Func<T1, Task<TResult>>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        vt.Item3.SetResult(await vt.Item2(vt.Item1));
                    }
                    catch (Exception ex)
                    {
                        vt.Item3.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(t1, action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous function with two state parameters and returns a <see cref="TaskCompletionSource{TResult}"/>
    /// that completes with the function's result. Exceptions are propagated to the returned TCS.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result produced by the asynchronous function.</typeparam>
    /// <param name="t1">First state value passed to the function.</param>
    /// <param name="t2">Second state value passed to the function.</param>
    /// <param name="action">Asynchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function result or faulted on exception.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, T2, TResult>(T1 t1, T2 t2, Func<T1, T2, Task<TResult>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<T1, T2, Func<T1, T2, Task<TResult>>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        TResult? result = await vt.Item3(vt.Item1, vt.Item2);

                        vt.Item4.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        vt.Item4.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(t1, t2, action, taskCompletionSource)
        );

        return taskCompletionSource;
    }

    /// <summary>
    /// Queues an asynchronous function with three state parameters and returns a <see cref="TaskCompletionSource{TResult}"/>
    /// that will be completed with the function's result when execution finishes. Any exception thrown by the
    /// function will be captured and set on the returned TaskCompletionSource.
    /// </summary>
    /// <typeparam name="T1">Type of the first state parameter.</typeparam>
    /// <typeparam name="T2">Type of the second state parameter.</typeparam>
    /// <typeparam name="T3">Type of the third state parameter.</typeparam>
    /// <typeparam name="TResult">Type of the result produced by the asynchronous function.</typeparam>
    /// <param name="t1">First state value to pass to the function.</param>
    /// <param name="t2">Second state value to pass to the function.</param>
    /// <param name="t3">Third state value to pass to the function.</param>
    /// <param name="action">Asynchronous function to execute. Cannot be null.</param>
    /// <returns>A TaskCompletionSource that will be completed with the function result or faulted if the function throws.</returns>
    public static TaskCompletionSource<TResult> QueueUserWorkItemAsync<T1, T2, T3, TResult>(T1 t1, T2 t2, T3 t3, Func<T1, T2, T3, Task<TResult>> action)
    {
        _ = action ?? throw new ArgumentNullException(nameof(action));

        TaskCompletionSource<TResult> taskCompletionSource = new();

        ThreadPool.QueueUserWorkItem(
            async o =>
            {
                if (o is ValueTuple<T1, T2, T3, Func<T1, T2, T3, Task<TResult>>, TaskCompletionSource<TResult>> vt)
                {
                    try
                    {
                        TResult? result = await vt.Item4(vt.Item1, vt.Item2, vt.Item3);

                        vt.Item5.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        vt.Item5.SetException(ex);
                    }
                }
            },
            ValueTuple.Create(t1, t2, t3, action, taskCompletionSource)
        );

        return taskCompletionSource;
    }
}
