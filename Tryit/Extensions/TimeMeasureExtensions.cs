using System.Diagnostics;

namespace System;

/// <summary>
/// Provides extension methods to measure the execution time of actions and functions, both synchronous and
/// asynchronous. Callbacks are invoked with the elapsed time in milliseconds.
/// </summary>
public static class TimeMeasureExtensions
{
    /// <summary>
    /// Measures the execution time of a specified action and invokes a callback with the elapsed time in milliseconds.
    /// </summary>
    /// <param name="invoker">The action to be executed and measured for its execution time.</param>
    /// <param name="timerCallback">The callback to be invoked with the elapsed time after the action has completed.</param>
    /// <exception cref="ArgumentNullException">Thrown when either the action to be executed or the callback for the elapsed time is null.</exception>
    public static void TimeMeasure(Action invoker, Action<int> timerCallback)
    {
        _ = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
        _ = invoker ?? throw new ArgumentNullException(nameof(invoker));

        Stopwatch stop = Stopwatch.StartNew();
        try
        {
            invoker.Invoke();
        }
        finally
        {
            stop.Stop();
            timerCallback.Invoke((int)stop.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Measures the execution time of a function and invokes a callback with the elapsed time in milliseconds.
    /// </summary>
    /// <typeparam name="T">Represents the return type of the function being measured.</typeparam>
    /// <param name="invoker">The function whose execution time is being measured.</param>
    /// <param name="timerCallback">A callback that receives the elapsed time in milliseconds after the function execution.</param>
    /// <returns>The result of the function being measured.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the function to measure or the callback is null.</exception>
    public static T TimeMeasure<T>(this Func<T> invoker, Action<int> timerCallback)
    {
        _ = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
        _ = invoker ?? throw new ArgumentNullException(nameof(invoker));

        Stopwatch stop = Stopwatch.StartNew();
        try
        {
            return invoker.Invoke();
        }
        finally
        {
            stop.Stop();
            timerCallback.Invoke((int)stop.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Measures the time taken to execute an asynchronous function and invokes a callback with the elapsed time in
    /// milliseconds.
    /// </summary>
    /// <param name="invoker">The asynchronous function to be executed and measured for its execution time.</param>
    /// <param name="timerCallback">A callback function that receives the elapsed time in milliseconds after the execution of the asynchronous
    /// function.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the asynchronous function or the callback function is null.</exception>
    public static async Task TimeMeasureAsync(Func<Task> invoker, Action<int> timerCallback)
    {
        _ = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
        _ = invoker ?? throw new ArgumentNullException(nameof(invoker));

        Stopwatch stop = Stopwatch.StartNew();

        try
        {
            await invoker.Invoke();
        }
        finally
        {
            stop.Stop();
            timerCallback.Invoke((int)stop.ElapsedMilliseconds);
        }
    }

    /// <summary>
    /// Measures the execution time of an asynchronous function and invokes a callback with the elapsed time in
    /// milliseconds.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned by the asynchronous function being measured.</typeparam>
    /// <param name="invoker">An asynchronous function whose execution time is to be measured.</param>
    /// <param name="timerCallback">A callback function that receives the elapsed time in milliseconds after the asynchronous function completes.</param>
    /// <returns>The result of the asynchronous function after it has been executed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the invoker or timerCallback is null.</exception>
    public static async Task<T> TimeMeasureAsync<T>(Func<Task<T>> invoker, Action<int> timerCallback)
    {
        _ = timerCallback ?? throw new ArgumentNullException(nameof(timerCallback));
        _ = invoker ?? throw new ArgumentNullException(nameof(invoker));

        Stopwatch stop = Stopwatch.StartNew();
        try
        {
            return await invoker.Invoke();
        }
        finally
        {
            stop.Stop();
            timerCallback.Invoke((int)stop.ElapsedMilliseconds);
        }
    }
}
