using System.Diagnostics;

namespace System.Diagnostics;

/// <summary>
/// Provides extension methods for measuring execution time of actions using a Stopwatch. Supports both synchronous and
/// asynchronous actions.
/// </summary>
public static class StopwatchExtensions
{
    /// <summary>
    /// Measures the time taken to execute a specified action using a stopwatch.
    /// </summary>
    /// <param name="stopwatch">An object that tracks elapsed time during the execution of the action.</param>
    /// <param name="action">A delegate representing the code to be executed and measured.</param>
    /// <param name="stopwatchRestart">Indicates whether to reset and start the stopwatch before measuring.</param>
    /// <returns>The total time elapsed during the execution of the action.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the stopwatch or the action is null.</exception>
    public static TimeSpan Measure(this Stopwatch stopwatch, Action action, bool stopwatchRestart = true)
    {
        _ = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        if (stopwatchRestart)
        {
            stopwatch.Reset();
            stopwatch.Restart();
        }

        try
        {
            action();
        }
        finally
        {
            stopwatch.Stop();
        }

        return stopwatch.Elapsed;
    }

    /// <summary>
    /// Measures the execution time of a specified action and returns the result along with the elapsed time.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result produced by the executed action.</typeparam>
    /// <param name="stopwatch">Used to track the elapsed time during the execution of the action.</param>
    /// <param name="action">The function to be executed and measured for its execution time.</param>
    /// <param name="stopwatchRestart">Indicates whether to reset and restart the stopwatch before measuring.</param>
    /// <returns>A tuple containing the result of the action and the time taken to execute it.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the stopwatch or the action is null.</exception>
    public static MeasureResult<T> Measure<T>(this Stopwatch stopwatch, Func<T> action, bool stopwatchRestart = true)
    {
        _ = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        if (stopwatchRestart)
        {
            stopwatch.Reset();
            stopwatch.Restart();
        }

        try
        {
            return new MeasureResult<T>(action(), stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Measures the execution time of an asynchronous action while optionally restarting a stopwatch.
    /// </summary>
    /// <typeparam name="T">Represents the type of the result returned by the asynchronous action being measured.</typeparam>
    /// <param name="stopwatch">Used to track the elapsed time during the execution of the asynchronous action.</param>
    /// <param name="action">The asynchronous function whose execution time is being measured.</param>
    /// <param name="stopwatchRestart">Indicates whether to reset and restart the stopwatch before measuring the action's execution time.</param>
    /// <returns>A tuple containing the result of the action and the time taken to execute it.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the stopwatch or the action is null.</exception>
    public static async Task<MeasureResult<T>> MeasureAsync<T>(this Stopwatch stopwatch, Func<Task<T>> action, bool stopwatchRestart = true)
    {
        _ = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        if (stopwatchRestart)
        {
            stopwatch.Reset();
            stopwatch.Restart();
        }

        try
        {
            var result = await action();

            return new MeasureResult<T>(result, stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Measures the time taken to execute an asynchronous action using a stopwatch.
    /// </summary>
    /// <param name="stopwatch">The timer used to measure the elapsed time during the execution of the action.</param>
    /// <param name="action">The asynchronous operation whose execution time is being measured.</param>
    /// <param name="stopwatchRestart">Indicates whether to reset and start the timer before measuring the action's execution time.</param>
    /// <returns>The total elapsed time after the action has completed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either the timer or the action to be measured is null.</exception>
    public static async Task<TimeSpan> MeasureAsync(this Stopwatch stopwatch, Func<Task> action, bool stopwatchRestart = true)
    {
        _ = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        _ = action ?? throw new ArgumentNullException(nameof(action));

        if (stopwatchRestart)
        {
            stopwatch.Reset();
            stopwatch.Restart();
        }

        try
        {
            await action();

            return (stopwatch.Elapsed);
        }
        finally
        {
            stopwatch.Stop();
        }
    }
}

/// <summary>
/// Encapsulates the outcome of an operation along with the time taken to complete it.
/// </summary>
/// <typeparam name="T">Represents the type of the result from an operation or computation.</typeparam>
public readonly struct MeasureResult<T>
{
    /// <summary>
    /// Constructs a new instance with a specified result and the time taken to achieve it.
    /// </summary>
    /// <param name="result">Holds the outcome of an operation or computation.</param>
    /// <param name="elapsed">Represents the duration taken to complete the operation.</param>
    public MeasureResult(T result, TimeSpan elapsed)
    {
        Result = result;
        Elapsed = elapsed;
    }

    /// <summary>
    /// Represents the result of an operation, providing access to the value of type T. It is a read-only property.
    /// </summary>
    public T Result { get; }

    /// <summary>
    /// Represents the total time that has elapsed since the start of an operation. It is a read-only property.
    /// </summary>
    public TimeSpan Elapsed { get; }

    /// <summary>
    /// Converts a StopwatchResult instance to its underlying result type. This allows for seamless type conversion.
    /// </summary>
    /// <param name="stopwatchResult">Represents the result of a timing operation, encapsulating the value to be converted.</param>

    public static implicit operator T(MeasureResult<T> stopwatchResult)
    {
        return stopwatchResult.Result;
    }
}
