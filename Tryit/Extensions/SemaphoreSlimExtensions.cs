using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Tryit;

internal static class SemaphoreSlimExtensions
{
    static SimpleObjectPool<ExclusiveLock> simpleObjectPool = SimpleObjectPool.Create<ExclusiveLock>(() => new ExclusiveLock(), i => i.semaphore = default!);

    public static IDisposable UseExclusiveLock(this SemaphoreSlim semaphoreSlim)
    {
        _ = semaphoreSlim ?? throw new ArgumentNullException(nameof(semaphoreSlim));

        semaphoreSlim.Wait();

        var exclusiveLock = simpleObjectPool.Rent();

        exclusiveLock.semaphore = semaphoreSlim;

        return exclusiveLock;
    }

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

    private class ExclusiveLock : IDisposable
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        internal SemaphoreSlim? semaphore;

        void IDisposable.Dispose()
        {
            if (Interlocked.Exchange(ref semaphore, null) is SemaphoreSlim slim)
            {
                simpleObjectPool.Return(this);
            }
        }
    }
}
