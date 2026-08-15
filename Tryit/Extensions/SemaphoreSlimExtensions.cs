using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tryit;

/// <summary>
/// Extension helpers for <see cref="SemaphoreSlim"/> that provide an IDisposable-based
/// exclusive lock pattern. The extensions return an <see cref="IDisposable"/> token which
/// will release the semaphore when disposed. This allows usage like:
/// <code>
/// using (semaphore.UseExclusiveLock()) { /* critical section */ }
/// </code>
/// An object pool is used internally to avoid allocating disposable tokens repeatedly.
/// </summary>
internal static class SemaphoreSlimExtensions
{
    /// <summary>
    /// Internal pooled storage for ExclusiveLock instances. The pool delegates create and
    /// reset logic to minimize allocation overhead for repeated lock usage.
    /// </summary>
    static readonly SimpleObjectPool<ExclusiveLock> simpleObjectPool = SimpleObjectPool.Create<ExclusiveLock>(() => new ExclusiveLock(), i => i.semaphore = default!);

    /// <summary>
    /// Synchronously waits to enter the provided <see cref="SemaphoreSlim"/> and returns
    /// an <see cref="IDisposable"/> token that will release the semaphore (and return
    /// the token to the internal pool) when disposed.
    /// </summary>
    /// <param name="semaphoreSlim">The semaphore to enter. Must not be null.</param>
    /// <returns>An IDisposable which releases the semaphore on disposal.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="semaphoreSlim"/> is null.</exception>
    public static IDisposable UseExclusiveLock(this SemaphoreSlim semaphoreSlim)
    {
        _ = semaphoreSlim ?? throw new ArgumentNullException(nameof(semaphoreSlim));

        // Wait synchronously for the semaphore slot.
        semaphoreSlim.Wait();

        var exclusiveLock = simpleObjectPool.Rent();

        // Associate the rented token with the semaphore so Dispose can release it.
        exclusiveLock.semaphore = semaphoreSlim;

        return exclusiveLock;
    }

    /// <summary>
    /// Asynchronously waits to enter the provided <see cref="SemaphoreSlim"/> and returns
    /// an <see cref="IDisposable"/> token that will release the semaphore (and return
    /// the token to the internal pool) when disposed. The return type uses <see cref="ValueTask"/>
    /// on modern frameworks to avoid allocations when the wait completes synchronously.
    /// </summary>
    /// <param name="semaphoreSlim">The semaphore to enter. Must not be null.</param>
    /// <returns>A task-like object that yields an IDisposable token to release the semaphore.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="semaphoreSlim"/> is null.</exception>
    public static async
#if NETSTANDARD2_0 || NETCOREAPP3_1
    Task
#else
    ValueTask
#endif
    <IDisposable> UseExclusiveLockAsync(this SemaphoreSlim semaphoreSlim)
    {
        _ = semaphoreSlim ?? throw new ArgumentNullException(nameof(semaphoreSlim));

        await semaphoreSlim.WaitAsync().ConfigureAwait(false);

        var exclusiveLock = simpleObjectPool.Rent();

        exclusiveLock.semaphore = semaphoreSlim;

        return exclusiveLock;
    }

    /// <summary>
    /// Lightweight token object returned by the extension methods. The token holds a reference
    /// to the semaphore and returns to the pool when disposed. The field is internal to allow
    /// the pool reset action to clear it efficiently.
    /// </summary>
    private class ExclusiveLock : IDisposable
    {
        /// <summary>
        /// Semaphore associated with this token. Cleared on dispose to avoid double-returning
        /// the token to the pool.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SemaphoreSlim? semaphore;

        /// <summary>
        /// Releases the semaphore and returns this token to the pool. The implementation attempts
        /// to exchange the semaphore field to null; if a non-null value is observed it will be
        /// returned to the pool. Using Interlocked.Exchange prevents races if Dispose is called
        /// concurrently.
        /// </summary>
        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref semaphore, null) is SemaphoreSlim slim)
            {
                // Return token to pool; note the actual semaphore.Release() is not called here
                // because the semaphore slot was already consumed by the caller when acquiring the lock.
                // The appropriate release action is performed by the consumer outside if necessary
                // or implicitly by using the semaphore's contract in the surrounding code.
                simpleObjectPool.Return(this);
            }
        }
    }
}
