using System.Collections.Concurrent;
using System.Diagnostics;

namespace Tryit;

/// <summary>
/// Provides static methods for creating and managing time anchors and decay anchors for measuring elapsed time
/// intervals.
/// </summary>
/// <remarks>XTimer enables the creation of time anchors, which can be used to measure elapsed time, and supports
/// associating anchors with string tokens for later retrieval. Thread safety is ensured when accessing anchors by
/// token. This class is intended for scenarios where precise timing and anchor management are required, such as
/// performance measurement or timeout handling.</remarks>
public static class XTimer
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static double FrequencyRato = ((double)TimeSpan.TicksPerSecond / Stopwatch.Frequency / TimeSpan.TicksPerMillisecond);

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly Stopwatch GLOBAL_STOP_WATCH = Stopwatch.StartNew();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    internal static readonly ConcurrentDictionary<string, TimeAnchor> timeAnchorMaps = new();

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private static int lockFlag = 0;

    /// <summary>
    /// Creates a new instance of the TimeAnchor class.
    /// </summary>
    /// <returns>A new TimeAnchor object representing the current anchor point in time.</returns>
    public static TimeAnchor SetAnchor()
    {
        return new TimeAnchor();
    }

    /// <summary>
    /// Creates a new time countdown anchor with the specified countdown duration in milliseconds.
    /// </summary>
    /// <param name="countdownMilliseconds">The countdown duration, in milliseconds, to be used for the time countdown anchor. Must be a non-negative value.</param>
    /// <returns>A new instance of <see cref="CountdownTimeAnchor"/> initialized with the specified countdown duration.</returns>
    public static CountdownTimeAnchor SetCountdownAnchor(long countdownMilliseconds)
    {
        return new CountdownTimeAnchor(countdownMilliseconds);
    }

    /// <summary>
    /// Creates and registers a new time anchor associated with the specified token, or uses the provided time anchor if
    /// given.
    /// </summary>
    /// <param name="timeAnchorToken">The unique token used to identify the time anchor. Cannot be null.</param>
    /// <param name="timeAnchor">An optional existing time anchor to associate with the token. If null, a new time anchor is created.</param>
    /// <returns>The time anchor instance associated with the specified token.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="timeAnchorToken"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a time anchor with the specified token already exists.</exception>
    public static TimeAnchor SetAnchor(string timeAnchorToken, TimeAnchor? timeAnchor = null)
    {
        _ = timeAnchorToken ?? throw new ArgumentNullException(nameof(timeAnchorToken));

        try
        {
            SpinWait spinWait = default!;

            while (Interlocked.CompareExchange(ref lockFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            if (timeAnchorMaps.ContainsKey(timeAnchorToken))
            {
                throw new InvalidOperationException($"Time anchor with token '{timeAnchorToken}' already exists.");
                // This check is necessary to prevent overwriting an existing time anchor, which could lead to unexpected behavior when retrieving anchors by token.
            }

            return (timeAnchorMaps[timeAnchorToken] = timeAnchor ?? new TimeAnchor());
        }
        finally
        {
            Interlocked.Exchange(ref lockFlag, 0);
        }
    }

    /// <summary>
    /// Retrieves and removes the time anchor associated with the specified token.
    /// </summary>
    /// <remarks>This method is thread-safe. Once a time anchor is retrieved using this method, it is removed
    /// and cannot be retrieved again with the same token.</remarks>
    /// <param name="timeAnchorToken">The unique token identifying the time anchor to retrieve. Cannot be null.</param>
    /// <returns>The time anchor associated with the specified token.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="timeAnchorToken"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if no time anchor exists for the specified token.</exception>
    public static TimeAnchor GetAnchor(string timeAnchorToken)
    {
        _ = timeAnchorToken ?? throw new ArgumentNullException(nameof(timeAnchorToken));

        try
        {
            SpinWait spinWait = default!;

            while (Interlocked.CompareExchange(ref lockFlag, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            if (timeAnchorMaps.TryRemove(timeAnchorToken, out TimeAnchor timeAnchor) == false)
            {
                throw new InvalidOperationException($"Time anchor with token '{timeAnchorToken}' does not exist.");
                // This check is necessary to ensure that the method behaves predictably when attempting to retrieve a time anchor that has not been set or has already been retrieved and removed.
            }
            return timeAnchor;
        }
        finally
        {
            Interlocked.Exchange(ref lockFlag, 0);
        }
    }
}

/// <summary>
/// Represents a reference point in time used to measure elapsed durations relative to its creation.
/// </summary>
/// <remarks>Use this struct to capture a moment in time and subsequently determine the elapsed time since that
/// moment. This is useful for timing operations or measuring intervals within an application. The struct is immutable
/// and thread-safe.</remarks>
public readonly struct TimeAnchor
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly long anchorTicks = XTimer.GLOBAL_STOP_WATCH.ElapsedTicks;

    /// <summary>
    /// Creates and starts a new time anchor instance representing the current point in time.
    /// </summary>
    /// <returns>A new <see cref="TimeAnchor"/> instance initialized to the current time.</returns>
    public static TimeAnchor StartNew()
    {
        return new TimeAnchor();
    }

    /// <summary>
    /// Initializes a new instance of the TimeAnchor class.
    /// </summary>
    public TimeAnchor() { }

    /// <summary>
    /// Gets the elapsed time since the timer was started.
    /// </summary>
    public TimeSpan Elapsed => TimeSpan.FromTicks(XTimer.GLOBAL_STOP_WATCH.ElapsedTicks - anchorTicks);

    /// <summary>
    /// Gets the number of elapsed milliseconds since the timer was started.
    /// </summary>
    /// <remarks>This value is calculated based on the difference in stopwatch ticks and may be affected by
    /// the timer's resolution. The result reflects the elapsed time as measured by the underlying timer
    /// implementation.</remarks>
    public long ElapsedMilliseconds => (long)((XTimer.GLOBAL_STOP_WATCH.ElapsedTicks - anchorTicks) * XTimer.FrequencyRato);
}

/// <summary>
/// Represents an anchor point in time used to track elapsed and remaining time for a decay period.
/// </summary>
/// <remarks>Use this struct to measure how much time has passed or remains before a specified decay duration
/// elapses. It is useful for scenarios where time-based expiration or decay logic is required, such as cache
/// invalidation or time-limited operations. The struct is immutable and thread-safe.</remarks>
public readonly struct CountdownTimeAnchor
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly long localTicks = XTimer.GLOBAL_STOP_WATCH.ElapsedTicks;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly long countdownMilliseconds;

    /// <summary>
    /// Creates a new instance of the TimeDecayAnchor class with the specified decay duration.
    /// </summary>
    /// <param name="decayMilliseconds">The decay duration, in milliseconds, to be used by the anchor. Must be a non-negative value.</param>
    /// <returns>A new TimeDecayAnchor instance configured with the specified decay duration.</returns>
    public static CountdownTimeAnchor StartNew(long decayMilliseconds)
    {
        return new CountdownTimeAnchor(decayMilliseconds);
    }

    /// <summary>
    /// Initializes a new instance of the CountdownTimeAnchor class with the specified countdown duration in milliseconds.
    /// </summary>
    /// <param name="countdownMilliseconds">The countdown duration, in milliseconds, used to control the rate at which values countdown over time. Must be greater
    /// than zero.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if countdownMilliseconds is less than or equal to zero.</exception>
    public CountdownTimeAnchor(long countdownMilliseconds)
    {
        if (countdownMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(countdownMilliseconds), "Elapsed milliseconds must be greater than zero.");
        }

        this.countdownMilliseconds = countdownMilliseconds;
    }

    /// <summary>
    /// Gets the amount of time remaining before the decay period elapses.
    /// </summary>
    /// <remarks>If the decay period has already elapsed, the returned value is zero. This property can be
    /// used to determine how much time is left before an associated operation or state expires.</remarks>
    public TimeSpan Remaining => TimeSpan.FromMilliseconds(Math.Max(0, countdownMilliseconds - ElapsedMilliseconds));

    /// <summary>
    /// Gets the number of milliseconds remaining before the decay period elapses.
    /// </summary>
    /// <remarks>Returns zero if the decay period has already elapsed.</remarks>
    public long RemainingMilliseconds => Math.Max(0, countdownMilliseconds - ElapsedMilliseconds);

    /// <summary>
    /// Gets the elapsed time since the timer was started.
    /// </summary>
    public TimeSpan Elapsed => TimeSpan.FromTicks(XTimer.GLOBAL_STOP_WATCH.ElapsedTicks - localTicks);

    /// <summary>
    /// Gets the number of elapsed milliseconds since the timer was started or last reset.
    /// </summary>
    /// <remarks>This value is calculated based on the underlying timer's tick count and may be affected by
    /// the timer's resolution. The returned value represents the elapsed time in milliseconds as a 64-bit
    /// integer.</remarks>
    public long ElapsedMilliseconds => (long)((XTimer.GLOBAL_STOP_WATCH.ElapsedTicks - localTicks) * XTimer.FrequencyRato);
}
