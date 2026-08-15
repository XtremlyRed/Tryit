using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Simple synchronized notifier that allows producers to notify a single or multiple consumers
/// about incoming items. Internally it uses a queue and a <see cref="SemaphoreSlim"/> to
/// coordinate asynchronous waiting and notification.
/// </summary>
/// <typeparam name="T">Type of items being passed through the notifier.</typeparam>
public class SyncNotify<T>
{
    /// <summary>
    /// Semaphore used to signal availability of items. Initial count is zero meaning consumers
    /// will wait until producers call <see cref="Notify"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(0);

    /// <summary>
    /// Internal queue that holds items produced by callers of <see cref="Notify"/> until a
    /// consumer dequeues them in <see cref="WaitAsync"/>.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Queue<T> queue = new Queue<T>();

    /// <summary>
    /// Simple spin-lock field used to protect <see cref="queue"/> access without full
    /// heavyweight locks. Value is 0 when unlocked and 1 when locked.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int locker;

    /// <summary>
    /// Asynchronously waits for the next available item. This method will block asynchronously
    /// until a producer calls <see cref="Notify(T)"/>. When signalled it will dequeue and return
    /// the next item from the internal queue.
    /// </summary>
    /// <returns>A task that completes with the next available item.</returns>
    public virtual async Task<T> WaitAsync()
    {
        // Loop to handle spurious wake-ups: only return when an item is actually present.
        AGAIN:
        await semaphoreSlim.WaitAsync().ConfigureAwait(false);

        try
        {
            // Acquire lightweight spin lock to access the queue
            SpinWait spinWait = default;

            while (Interlocked.CompareExchange(ref locker, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }
        }
        finally
        {
            // release lock
            Interlocked.Exchange(ref locker, 0);
        }

        // If nothing was in queue after being signalled, wait again.
        goto AGAIN;
    }

    /// <summary>
    /// Notifies the notifier that a new item is available. This method will enqueue the item and
    /// release the internal semaphore so a waiting consumer can resume and obtain the item.
    /// </summary>
    /// <param name="item">Item to publish to waiting consumers.</param>
    public virtual void Notify(T item)
    {
        try
        {
            // Acquire lightweight spin lock to access the queue
            SpinWait spinWait = default;

            while (Interlocked.CompareExchange(ref locker, 1, 0) != 0)
            {
                spinWait.SpinOnce();
            }

            queue.Enqueue(item);
        }
        finally
        {
            // release lock
            Interlocked.Exchange(ref locker, 0);
        }

        // Signal a waiter that an item is available.
        semaphoreSlim.Release();
    }
}

#if !NETSTANDARD2_0

/// <summary>
/// Extension helpers for <see cref="SyncNotify{T}"/>. These helpers require a runtime that
/// supports IAsyncEnumerable (not available on .NET Standard 2.0 in this project).
/// </summary>
public static class SyncNotifyExtensions
{
    /// <summary>
    /// Generates an infinite <see cref="IAsyncEnumerable{T}"/> that yields items produced via
    /// <see cref="SyncNotify{T}.Notify(T)"/>. Consumers can await enumeration to receive items
    /// as they arrive.
    /// </summary>
    /// <typeparam name="T">Item type.</typeparam>
    /// <param name="syncNotify">The sync notify instance to enumerate from.</param>
    /// <returns>An async enumerable producing items as they are notified.</returns>
    public static async IAsyncEnumerable<T> GenerateEnumerableAsync<T>(this SyncNotify<T> syncNotify)
    {
        while (true)
        {
            T value = await syncNotify.WaitAsync().ConfigureAwait(false);

            yield return value;
        }
    }
}

#endif
